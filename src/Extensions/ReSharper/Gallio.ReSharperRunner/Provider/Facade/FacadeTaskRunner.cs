// Copyright 2005-2008 Gallio Project - http://www.gallio.org/
// Portions Copyright 2000-2004 Jonathan de Halleux
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Threading;
using Gallio.Loader;
using JetBrains.ReSharper.TaskRunnerFramework;

namespace Gallio.ReSharperRunner.Provider.Facade
{
    /// <summary>
    /// <para>
    /// Runs remote tasks via the facade.
    /// </para>
    /// <para>
    /// The implementation creates a facade for the ReSharper interfaces used
    /// by Gallio then kicks off the work in a fresh AppDomain.  This ensures
    /// that the Gallio runtime environment is not polluted with test assemblies
    /// or ReSharper dependencies that may interfere with test execution.
    /// </para>
    /// </summary>
    /// <remarks>
    /// This type is part of a facade that decouples the Gallio test runner from the ReSharper interfaces.
    /// </remarks>
    public class FacadeTaskRunner : RecursiveRemoteTaskRunner
    {
        private TaskResult executeResult;

        public FacadeTaskRunner(IRemoteTaskServer server)
            : base(server)
        {
            executeResult = TaskResult.Error;
        }

        sealed public override void ConfigureAppDomain(TaskAppDomainConfiguration configuration)
        {
            configuration.ApartmentState = ApartmentState.STA;
        }

        sealed public override TaskResult Start(TaskExecutionNode node)
        {
            return TaskResult.Success; 
        }

        sealed public override TaskResult Execute(TaskExecutionNode node)
        {
            throw new NotImplementedException("Should not be called.");
        }

        sealed public override TaskResult Finish(TaskExecutionNode node)
        {
            return executeResult;
        }

        sealed public override void ExecuteRecursive(TaskExecutionNode node)
        {
            AdapterFacadeTaskServer facadeTaskServer = new AdapterFacadeTaskServer(Server);

            FacadeTaskExecutorConfiguration config = new FacadeTaskExecutorConfiguration()
            {
                ShadowCopy = TaskExecutor.Configuration.ShadowCopy,
                AssemblyFolder = TaskExecutor.Configuration.AssemblyFolder
            };

            FacadeTask facadeTask = facadeTaskServer.MapTasks(node);
            executeResult = FacadeUtils.ToTaskResult(Execute(facadeTaskServer, facadeTask, config));
        }

        protected virtual FacadeTaskResult Execute(IFacadeTaskServer server, FacadeTask task, FacadeTaskExecutorConfiguration config)
        {
            IGallioRemoteEnvironment environment = EnvironmentManager.GetSharedEnvironment();

            Type taskRunnerType = typeof(RemoteFacadeTaskRunner);
            IRemoteFacadeTaskRunner taskRunner = (IRemoteFacadeTaskRunner)environment.AppDomain.CreateInstanceAndUnwrap(
                taskRunnerType.Assembly.FullName, taskRunnerType.FullName);

            return taskRunner.Execute(server, task, config);
        }
    }
}

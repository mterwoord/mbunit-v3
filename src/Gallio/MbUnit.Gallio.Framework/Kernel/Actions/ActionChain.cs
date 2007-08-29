using System;
using System.Collections.Generic;
using System.Text;

namespace MbUnit.Framework.Kernel.Actions
{
    /// <summary>
    /// An action chain captures a sequence of actions to be performed as
    /// part of a complex multi-part process.
    /// </summary>
    /// <remarks>
    /// Later actions in a chain will not run if prior ones throw an exception.
    /// To catch and handle exceptions, use <see cref="Around" /> to register a
    /// decorator that runs around the current action whose execution is then
    /// under your control.
    /// </remarks>
    public class ActionChain<T>
    {
        private Action<T> action;

        /// <summary>
        /// Creates an empty action chain.
        /// </summary>
        public ActionChain()
        {
        }

        /// <summary>
        /// Gets a singleton action that does nothing when invoked.
        /// </summary>
        public static readonly Action<T> NoOp = delegate { };

        /// <summary>
        /// <para>
        /// Gets or sets a representation of the chain as a single action.
        /// </para>
        /// <para>
        /// The action is progressively augmented as new contributions are
        /// registered using <see cref="Before" />, <see cref="After" /> and
        /// <see cref="Around" />.  By default the action is <see cref="NoOp" />.
        /// </para>
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null</exception>
        public Action<T> Action
        {
            get
            {
                return action ?? NoOp;
            }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");

                action = value == NoOp ? null : value;
            }
        }

        /// <summary>
        /// Registers an action to perform before all other actions
        /// currently in the chain.
        /// </summary>
        /// <remarks>
        /// The value of <see cref="Action" /> will be set to a new instance
        /// that performs the specified <paramref name="action"/> before
        /// the current <see cref="Action" />.
        /// </remarks>
        /// <param name="action">The action to register</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="action"/> is null</exception>
        public void Before(Action<T> action)
        {
            if (action == null)
                throw new ArgumentNullException("action");

            if (this.action == null)
                this.action = action;
            else
                this.action = (Action<T>) Action<T>.Combine(action, this.action);
        }

        /// <summary>
        /// Registers an action to perform after all other actions
        /// currently in the chain.
        /// </summary>
        /// <remarks>
        /// The value of <see cref="Action" /> will be set to a new instance
        /// that performs the specified <paramref name="action"/> after
        /// the current <see cref="Action" />.
        /// </remarks>
        /// <param name="action">The action to register</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="action"/> is null</exception>
        public void After(Action<T> action)
        {
            if (action == null)
                throw new ArgumentNullException("action");

            if (this.action == null)
                this.action = action;
            else
                this.action = (Action<T>)Action<T>.Combine(this.action, action);
        }

        /// <summary>
        /// Registers an action to perform around all other actions
        /// currently in the chain.  The contained part of the chain
        /// is passed in as an action to the decorator that the decorator
        /// can choose to run -- or not -- as needed.
        /// </summary>
        /// <remarks>
        /// The value of <see cref="Action" /> will be set to a new instance
        /// that performs the specified <paramref name="decorator"/> around
        /// the current <see cref="Action" />.
        /// </remarks>
        /// <param name="decorator">The decorator to register</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="decorator"/> is null</exception>
        public void Around(ActionDecorator<T> decorator)
        {
            if (decorator == null)
                throw new ArgumentNullException("decorator");

            Action<T> innerAction = Action;
            action = delegate(T obj)
            {
                decorator(obj, innerAction);
            };
        }
    }
}

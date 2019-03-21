#region License

/*
 * Copyright 2002-2009 the original author or authors.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      https://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF Any KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using Spring.Integration.Core;
using Spring.Threading.Collections.Generic;
using Spring.Util;

namespace Spring.Integration.Selector {
    /// <summary>
    /// A message selector implementation that passes incoming messages through a
    /// chain of selectors. Whether the Message is {@link #accept(Message) accepted}
    /// is based upon the tallied results of the individual selectors' responses in
    /// accordance with this chain's {@link VotingStrategyKind}.
    /// </summary>
    /// <author>Mark Fisher</author>
    /// <author>Andreas D�hring (.NET)</author>
    public class MessageSelectorChain : IMessageSelector {

        /// <summary>
        /// represent a voting strategy
        /// </summary>
        public enum VotingStrategyKind {
            /// <summary>
            /// all messages selectors must accept the message
            /// </summary>
            All, 
            /// <summary>
            /// at least one message selecxtor must accept the message
            /// </summary>
            Any, 
            /// <summary>
            /// more then the half of the message selectors must accept the message
            /// </summary>
            Majority, 
            /// <summary>
            /// at least the half of the message selectors must accept the message
            /// </summary>
            MajorityOrTie
        };


        private volatile VotingStrategyKind _votingStrategy = VotingStrategyKind.All;

        private readonly IList<IMessageSelector> _selectors = new CopyOnWriteArrayList<IMessageSelector>();


        /// <summary>
        /// Specify the voting strategy for this selector chain.
        /// <p>The default is {@link VotingStrategyKind#All}.
        /// </summary>         
        public VotingStrategyKind VotingStrategy {
            set {
                AssertUtils.ArgumentNotNull(value, "voting must not be null");
                _votingStrategy = value;
            }
        }

        /// <summary>
        /// Add a selector to the end of the chain.
        /// </summary>
        /// <param name="selector"></param>
        public void Add(IMessageSelector selector) {
            _selectors.Add(selector);
        }

        /// <summary>
        /// Add a selector to the chain at the specified index.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="selector"></param>
        public void Add(int index, IMessageSelector selector) {
            _selectors[index] = selector;
        }

        ///// <summary>
        ///// Initialize the selector chain. Removes any existing selectors.
        ///// </summary>
        //public IList<IMessageSelector> Selectors {
        //    set {
        //        AssertUtils.ArgumentNotNull(value, "selectors must not be empty");
                
        //        lock(_selectors) {
        //            _selectors.Clear();
        //            foreach(IMessageSelector selector in value)
        //                _selectors.Add(selector);
        //        }
        //    }
        //}

        /// <summary>
        /// Initialize the selector chain. Removes any existing selectors.
        /// </summary>
        public IList<IMessageSelector> Selectors {
            set {
                AssertUtils.ArgumentNotNull(value, "selectors must not be empty");

                lock(_selectors) {
                    _selectors.Clear();
                    foreach(IMessageSelector selector in value)
                        _selectors.Add(selector);
                }
            }
        }

        /// <summary>
        /// Pass the message through the selector chain. Whether the Message is
        /// {@link #accept(Message) accepted} is based upon the tallied results of
        /// the individual selectors' responses in accordance with this chain's
        /// {@link VotingStrategyKind}.
        /// </summary>
        /// <param name="message">the message to check</param>
        /// <returns><c>true</c> if the message was accepted ohterwise <c>false</c></returns>
        public bool Accept(IMessage message) {
            int count = 0;
            int accepted = 0;
            foreach(IMessageSelector next in _selectors) {
                count++;
                if(next.Accept(message)) {
                    if(_votingStrategy.Equals(VotingStrategyKind.Any)) {
                        return true;
                    }
                    accepted++;
                }
                else if(_votingStrategy.Equals(VotingStrategyKind.All)) {
                    return false;
                }
            }
            return Decide(accepted, count);
        }

        private bool Decide(int accepted, int total) {
            if(accepted == 0) {
                return false;
            }
            switch(_votingStrategy) {
                case VotingStrategyKind.Any:
                    return true;
                case VotingStrategyKind.All:
                    return (accepted == total);
                case VotingStrategyKind.Majority:
                    return (2 * accepted) > total;
                case VotingStrategyKind.MajorityOrTie:
                    return (2 * accepted) >= total;
                default:
                    throw new ArgumentException("unsupported voting strategy " + _votingStrategy);
            }
        }
    }
}

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

#region

using System.Collections.Generic;
using NUnit.Framework;
using Spring.Context;
using Spring.Integration.Message;
using Spring.Integration.Selector;
using Spring.Integration.Tests.Util;

#endregion

namespace Spring.Integration.Tests.Config
{
    /// <author>Mark Fisher</author>
    /// <author>Andreas D�hring (.NET)</author>
    [TestFixture]
    public class SelectorChainParserTests
    {
        [Test]
        public void SelectorChain()
        {
            IApplicationContext context = TestUtils.GetContext(@"Config\selectorChainParserTests.xml");
            IMessageSelector selector1 = (IMessageSelector) context.GetObject("selector1");
            IMessageSelector selector2 = (IMessageSelector) context.GetObject("selector2");
            MessageSelectorChain chain = (MessageSelectorChain) context.GetObject("selectorChain");
            IList<IMessageSelector> selectors = GetSelectors(chain);
            Assert.That(GetStrategy(chain), Is.EqualTo(MessageSelectorChain.VotingStrategyKind.All));
            Assert.That(selectors[0], Is.EqualTo(selector1));
            Assert.That(selectors[1], Is.EqualTo(selector2));
            Assert.IsTrue(chain.Accept(new StringMessage("test")));
        }

        [Test]
        public void NestedSelectorChain()
        {
            IApplicationContext context = TestUtils.GetContext(@"Config\selectorChainParserTests.xml");
            IMessageSelector selector1 = (IMessageSelector) context.GetObject("selector1");
            IMessageSelector selector2 = (IMessageSelector) context.GetObject("selector2");
            IMessageSelector selector3 = (IMessageSelector) context.GetObject("selector3");
            IMessageSelector selector4 = (IMessageSelector) context.GetObject("selector4");
            IMessageSelector selector5 = (IMessageSelector) context.GetObject("selector5");
            IMessageSelector selector6 = (IMessageSelector) context.GetObject("selector6");
            MessageSelectorChain chain1 = (MessageSelectorChain) context.GetObject("nestedSelectorChain");
            Assert.That(GetStrategy(chain1), Is.EqualTo(MessageSelectorChain.VotingStrategyKind.Majority));
            IList<IMessageSelector> selectorList1 = GetSelectors(chain1);
            Assert.That(selectorList1[0], Is.EqualTo(selector1));
            Assert.IsTrue(selectorList1[1] is MessageSelectorChain);
            MessageSelectorChain chain2 = (MessageSelectorChain) selectorList1[1];
            Assert.That(GetStrategy(chain2), Is.EqualTo(MessageSelectorChain.VotingStrategyKind.All));
            IList<IMessageSelector> selectorList2 = GetSelectors(chain2);
            Assert.That(selectorList2[0], Is.EqualTo(selector2));
            Assert.IsTrue(selectorList2[1] is MessageSelectorChain);
            MessageSelectorChain chain3 = (MessageSelectorChain) selectorList2[1];
            Assert.That(GetStrategy(chain3), Is.EqualTo(MessageSelectorChain.VotingStrategyKind.Any));
            IList<IMessageSelector> selectorList3 = GetSelectors(chain3);
            Assert.That(selectorList3[0], Is.EqualTo(selector3));
            Assert.That(selectorList3[1], Is.EqualTo(selector4));
            Assert.That(selectorList2[2], Is.EqualTo(selector5));
            Assert.IsTrue(selectorList1[2] is MessageSelectorChain);
            MessageSelectorChain chain4 = (MessageSelectorChain) selectorList1[2];
            Assert.That(GetStrategy(chain4), Is.EqualTo(MessageSelectorChain.VotingStrategyKind.MajorityOrTie));
            IList<IMessageSelector> selectorList4 = GetSelectors(chain4);
            Assert.That(selectorList4[0], Is.EqualTo(selector6));
            Assert.IsTrue(chain1.Accept(new StringMessage("test1")));
            Assert.IsTrue(chain2.Accept(new StringMessage("test2")));
            Assert.IsTrue(chain3.Accept(new StringMessage("test3")));
            Assert.IsTrue(chain4.Accept(new StringMessage("test4")));
        }


        private static IList<IMessageSelector> GetSelectors(MessageSelectorChain chain)
        {
            return (IList<IMessageSelector>) TestUtils.GetFieldValue(chain, "_selectors");
        }

        private static MessageSelectorChain.VotingStrategyKind GetStrategy(MessageSelectorChain chain)
        {
            return (MessageSelectorChain.VotingStrategyKind) TestUtils.GetFieldValue(chain, "_votingStrategy");
        }
    }
}
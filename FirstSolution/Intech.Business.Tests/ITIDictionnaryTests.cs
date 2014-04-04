﻿using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intech.Business.Tests
{
    [TestFixture]
    class ITIDictionnaryTests
    {
        [Test]
        public void ourDictionaryTestIsNotPerfect()
        {
            ITIDictionary<string, int> dic = new ITIDictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            dic.Add("Paul", 435);
            Assert.That(dic["Paul"] == 435);

            Assert.That(dic["paul"] == 435);

            foreach (KeyValuePair<string, int> pair in dic)
            {
                Console.WriteLine(pair.Key + " - " + pair.Value);
            }

            foreach (var value in dic.Values)
            {
                Console.WriteLine(value);
            }
        }
    }
}

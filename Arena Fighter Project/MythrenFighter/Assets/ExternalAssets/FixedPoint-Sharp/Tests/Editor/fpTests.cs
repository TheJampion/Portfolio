﻿using FixedPoint;
using NUnit.Framework;

namespace FPTesting
{
    public class fpTests
    {
        [Test]
        public void ToStringTest()
        {
            var originalFp = fp._1 - fp._0_01;
            Assert.That(originalFp.ToString(), Is.EqualTo("0.99001"));
            
            originalFp = fp._1 - fp._0_01 *fp._0_01;
            Assert.That(originalFp.ToString(), Is.EqualTo("0.99991"));
            
            originalFp = fp._1;
            Assert.That(originalFp.ToString(), Is.EqualTo("1.00000"));
            
            originalFp = fp._1 + fp._0_01;
            Assert.That(originalFp.ToString(), Is.EqualTo("1.00999"));
            
            originalFp = fp._0_01;
            Assert.That(originalFp.ToString(), Is.EqualTo("0.00999"));
            
            originalFp = fp._0_50;
            Assert.That(originalFp.ToString(), Is.EqualTo("0.50000"));
        }
        
        [Test]
        public void FromStringTest()
        {
            var parsedFp = fp.ParseUnsafe("4.05");
            Assert.IsTrue(parsedFp >fp._4           +fp._0_04);
            Assert.IsTrue(parsedFp <fp._4 +fp._0_05 +fp._0_01);

            parsedFp = fp.ParseUnsafe("334535.98767");
            Assert.IsTrue(parsedFp > fp.Parse(334535)            + fp._0_95);
            Assert.IsTrue(parsedFp < fp.Parse(334535) + fp._0_95 + fp._0_04);
            
            parsedFp = fp.ParseUnsafe("-.00005");
            Assert.IsTrue(parsedFp < fp._0);
            Assert.IsTrue(parsedFp > -fp._0_01 *fp._0_01);
        }

        [Test]
        public void FromFloatTest()
        {
            var parsedFp = fp.ParseUnsafe(4.5f);
            Assert.IsTrue(parsedFp > fp._4 + fp._0_50 - fp._0_02);
            Assert.IsTrue(parsedFp < fp._4 + fp._0_50 + fp._0_01);
            
            parsedFp = fp.ParseUnsafe(335.978655f);
            Assert.IsTrue(parsedFp > fp.Parse(335)            + fp._0_95);
            Assert.IsTrue(parsedFp < fp.Parse(335) + fp._0_95 + fp._0_04);
            
            parsedFp = fp.ParseUnsafe(-.00005f);
            Assert.IsTrue(parsedFp < fp._0);
            Assert.IsTrue(parsedFp > -fp._0_01 *fp._0_01);
        }

        [Test]
        public void AsIntTest() {
            var val = fp._0_25 + fp._1;
            Assert.That(val.AsInt, Is.EqualTo(1));
            
            val = -fp._0_25 - fp._1;
            Assert.That(val.AsInt, Is.EqualTo(-2));
        }
    }
}
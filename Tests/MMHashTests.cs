﻿using System;
using System.Text;
using NUnit.Framework;
using DarkCaster.Hash;

namespace Tests
{
	[TestFixture]
	public class MMHashTests
	{
		[Test]
		public void ComplianceTests()
		{
			Assert.AreEqual(0x00000000,MMHash32.GetHash(0x00000000,new byte[0]));
			Assert.AreEqual(0x6a396f08,MMHash32.GetHash(0xFBA4C795,new byte[0]));
			Assert.AreEqual(0x81f16f39,MMHash32.GetHash(0xffffffff,new byte[0]));

			Assert.AreEqual(0x514e28b7,MMHash32.GetHash(0x00000000,new byte[] { 0x00 }));
			Assert.AreEqual(0xea3f0b17,MMHash32.GetHash(0xFBA4C795,new byte[] { 0x00 }));
			Assert.AreEqual(0xfd6cf10d,MMHash32.GetHash(0x00000000,new byte[] { 0xff }));

			Assert.AreEqual(0x16c6b7ab,MMHash32.GetHash(0x00000000,new byte[] { 0x00, 0x11 }));
			Assert.AreEqual(0x8eb51c3d,MMHash32.GetHash(0x00000000,new byte[] { 0x00, 0x11, 0x22 }));
			Assert.AreEqual(0xb4471bf8,MMHash32.GetHash(0x00000000,new byte[] { 0x00, 0x11, 0x22, 0x33 }));
			Assert.AreEqual(0xe2301fa8,MMHash32.GetHash(0x00000000,new byte[] { 0x00, 0x11, 0x22, 0x33, 0x44 }));
			Assert.AreEqual(0xfc2e4a15,MMHash32.GetHash(0x00000000,new byte[] { 0x00, 0x11, 0x22, 0x33, 0x44, 0x55 }));
			Assert.AreEqual(0xb074502c,MMHash32.GetHash(0x00000000,new byte[] { 0x00, 0x11, 0x22, 0x33, 0x44, 0x55, 0x66 }));
			Assert.AreEqual(0x8034d2a0,MMHash32.GetHash(0x00000000,new byte[] { 0x00, 0x11, 0x22, 0x33, 0x44, 0x55, 0x66, 0x77 }));
			Assert.AreEqual(0xb4698def,MMHash32.GetHash(0x00000000,new byte[] { 0x00, 0x11, 0x22, 0x33, 0x44, 0x55, 0x66, 0x77, 0x88 }));

			var textAsBytes=Encoding.ASCII.GetBytes("The quick brown fox jumps over the lazy dog");
			Assert.AreEqual(0x2E4FF723,MMHash32.GetHash(0x00000000,textAsBytes));
			Assert.AreEqual(0xC09DC139,MMHash32.GetHash(0x0000029A,textAsBytes));
			
			Assert.AreEqual(0x517F9467,MMHash32.GetHash(0x51c757e7,textAsBytes));
			textAsBytes=Encoding.ASCII.GetBytes("The quick brown fox jumps over the lazy cog");
			Assert.AreEqual(0x48B6D83F,MMHash32.GetHash(0x51c757e7,textAsBytes));
		}
	}
}

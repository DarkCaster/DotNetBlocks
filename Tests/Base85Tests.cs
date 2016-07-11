using System;
using System.Text;
using System.IO;
using NUnit.Framework;
using DarkCaster.Converters;

namespace Tests
{
	[TestFixture]
	public class Base85Tests
	{
		[Test]
		public void SampleDecode()
		{
			// https://en.wikipedia.org/wiki/Ascii85
			string encdata =
				"<~9jqo^BlbD-BleB1DJ+*+F(f,q/0JhKF<GL>Cj@.4Gp$d7F!,L7@<6@)/0JDEF<G%<+EV:2F!," +
				"O<DJ+*.@<*K0@<6L(Df-\\0Ec5e;DffZ(EZee.Bl.9pF\"AGXBPCsi+DGm>@3BB/F*&OCAfu2/AKY" +
				"i(DIb:@FD,*)+C]U=@3BN#EcYf8ATD3s@q?d$AftVqCh[NqF<G:8+EV:.+Cf>-FD5W8ARlolDIa" +
				"l(DId<j@<?3r@:F%a+D58'ATD4$Bl@l3De:,-DJs`8ARoFb/0JMK@qB4^F!,R<AKZ&-DfTqBG%G" +
				">uD.RTpAKYo'+CT/5+Cei#DII?(E,9)oF*2M7/c~>";
			byte[] result = Base85.Decode(encdata, true);
			string sresult = System.Text.Encoding.Default.GetString(result);
			string check =
				"Man is distinguished, not only by his reason, but by this singular passion from other " +
				"animals, which is a lust of the mind, that by a perseverance of delight in the continued and " +
				"indefatigable generation of knowledge, exceeds the short vehemence of any carnal pleasure.";
			Assert.AreEqual(check, sresult);
		}

		void CheckString(string target, bool marks)
		{
			var encoding = Encoding.GetEncoding("us-ascii", new EncoderReplacementFallback(" "), new DecoderReplacementFallback(" "));
			var data = encoding.GetBytes(target);
			for(int i = 0; i < data.Length; ++i)
				if(marks)
				{
					if(data[i] != 122 && data[i] != 126 && (data[i] < 33 || data[i] > 117))
						Assert.Fail("Encoded string has incorrect symbol: " + data[i].ToString());
				}
				else
				{
					if(data[i] != 122 && (data[i] < 33 || data[i] > 117))
						Assert.Fail("Encoded string has incorrect symbol: " + data[i].ToString());
				}
			var recheck = encoding.GetString(data);
			Assert.AreEqual(target, recheck);
		}

		[Test]
		public void RandomEncodeDecode()
		{
			var random = new Random();
			const int minSrcLen = 1024;
			const int maxSrcLen = 4096;
			for(int i = 0; i < 100; ++i)
			{
				var marks = Convert.ToBoolean(random.Next(0, 2));
				var source = new byte[random.Next(minSrcLen, maxSrcLen)];
				random.NextBytes(source);
				var result = Base85.Encode(source, marks);
				var restored = Base85.Decode(result, marks);
				Assert.AreEqual(source, restored);
				CheckString(result, marks);
			}
		}

		[Test]
		public void ComprEncodeDecode()
		{
			var random = new Random();
			const int minUncChunkLen = 1;
			const int maxUncChunkLen = 32;
			const int minComprChunkLen = 1;
			const int maxComprChunkLen = 16;
			const int chunkCount = 256;
			for(int i = 0; i < 100; ++i)
			{
				var marks = Convert.ToBoolean(random.Next(0, 2));
				byte[] source;
				using(var comprStream = new MemoryStream())
				{
					for(int j = 0; j < chunkCount; ++j)
					{
						var compr=Convert.ToBoolean(random.Next(0, 2));
						if(compr)
						{
							var len = random.Next(minComprChunkLen, maxComprChunkLen);
							for(int k = 0; k < len; ++k)
								comprStream.WriteByte(0);
						}
						else
						{
							var len = random.Next(minUncChunkLen, maxUncChunkLen);
							for(int k = 0; k < len; ++k)
								comprStream.WriteByte((byte)random.Next(0, 256));
						}
					}
					source = comprStream.ToArray();
				}
				var result = Base85.Encode(source, marks);
				var restored = Base85.Decode(result, marks);
				Assert.AreEqual(source, restored);
				CheckString(result, marks);
			}
		}

		[Test]
		public void DecodeBrokenString()
		{
			var random = new Random();
			const int minSrcLen = 128;
			const int maxSrcLen = 256;
			for(int i = 0; i < 100; ++i)
			{
				var marks = Convert.ToBoolean(random.Next(0, 2));
				//original source data
				var source = new byte[random.Next(minSrcLen, maxSrcLen)];
				random.NextBytes(source);
				//create encoded string and convert it to ascii bytes
				var encoding = Encoding.GetEncoding("us-ascii", new EncoderReplacementFallback(" "), new DecoderReplacementFallback(" "));
				var goodResult = encoding.GetBytes(Base85.Encode(source, marks));
				string badResult;
				//randomly insert illegal characters into stream
				using(var badStream = new MemoryStream())
				{
					var startPos = (marks ? 2 : 0);
					var endPos = (marks ? goodResult.Length - 2 : goodResult.Length);
					if(marks)
						badStream.Write(goodResult, 0, 2);
					for(int j = startPos; j < endPos; ++j)
					{
						var spoil = Convert.ToBoolean(random.Next(0, 2));
						while(spoil)
						{
							byte ch = (byte)random.Next(0, 256);
							while(ch == 122 || (ch >= 33 && ch <= 117))
								ch = (byte)random.Next(0, 256);
							badStream.WriteByte(ch);
							spoil = Convert.ToBoolean(random.Next(0, 2));
						}
						badStream.Write(goodResult, j, 1);
					}
					if(marks)
						badStream.Write(goodResult, goodResult.Length - 2, 2);
					var badArray = badStream.ToArray();
					var badCharArray = new char[badArray.Length];
					for(int j = 0; j < badArray.Length; ++j)
						badCharArray[j] = (char)badArray[j];
					badResult = new string(badCharArray);
				}
				var restored = Base85.Decode(badResult, marks, true);
				Assert.AreEqual(source, restored);
			}
		}
	}
}


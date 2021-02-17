/*
Copyright (c) 2021 Michal Turecki
All rights reserved.
Redistribution and use in source and binary forms, with or without
modification, are permitted provided that the following conditions
are met:
1. Redistributions of source code must retain the above copyright
   notice, this list of conditions and the following disclaimer.
2. Redistributions in binary form must reproduce the above copyright
   notice, this list of conditions and the following disclaimer in the
   documentation and/or other materials provided with the distribution.
3. Neither the name of copyright holders nor the names of its
   contributors may be used to endorse or promote products derived
   from this software without specific prior written permission.
THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
``AS IS'' AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED
TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR
PURPOSE ARE DISCLAIMED.  IN NO EVENT SHALL COPYRIGHT HOLDERS OR CONTRIBUTORS
BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR
CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN
CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE
POSSIBILITY OF SUCH DAMAGE.

Based on php forceutf8 library by Sebastián Grignoli: https://github.com/neitanod/forceutf8
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ForceUtf8Sharp
{
    public static class ForceUtf8
    {
        private static readonly Dictionary<int, byte[]> Win1252ToUtf8 = new Dictionary<int, byte[]>
        {
            { 128, new byte[] { 0xe2, 0x82, 0xac } },
            { 130, new byte[] { 0xe2, 0x80, 0x9a } },
            { 131, new byte[] { 0xc6, 0x92 } },
            { 132, new byte[] { 0xe2, 0x80, 0x9e } },
            { 133, new byte[] { 0xe2, 0x80, 0xa6 } },
            { 134, new byte[] { 0xe2, 0x80, 0xa0 } },
            { 135, new byte[] { 0xe2, 0x80, 0xa1 } },
            { 136, new byte[] { 0xcb, 0x86 } },
            { 137, new byte[] { 0xe2, 0x80, 0xb0 } },
            { 138, new byte[] { 0xc5, 0xa0 } },
            { 139, new byte[] { 0xe2, 0x80, 0xb9 } },
            { 140, new byte[] { 0xc5, 0x92 } },
            { 142, new byte[] { 0xc5, 0xbd } },
            { 145, new byte[] { 0xe2, 0x80, 0x98 } },
            { 146, new byte[] { 0xe2, 0x80, 0x99 } },
            { 147, new byte[] { 0xe2, 0x80, 0x9c } },
            { 148, new byte[] { 0xe2, 0x80, 0x9d } },
            { 149, new byte[] { 0xe2, 0x80, 0xa2 } },
            { 150, new byte[] { 0xe2, 0x80, 0x93 } },
            { 151, new byte[] { 0xe2, 0x80, 0x94 } },
            { 152, new byte[] { 0xcb, 0x9c } },
            { 153, new byte[] { 0xe2, 0x84, 0xa2 } },
            { 154, new byte[] { 0xc5, 0xa1 } },
            { 155, new byte[] { 0xe2, 0x80, 0xba } },
            { 156, new byte[] { 0xc5, 0x93 } },
            { 158, new byte[] { 0xc5, 0xbe } },
            { 159, new byte[] { 0xc5, 0xb8 } }
        };

        private static readonly Dictionary<uint, byte[]> BrokenUtf8ToUtf8 = new Dictionary<uint, byte[]>
        {
            { 0x80c2, new byte[] { 0xe2, 0x82, 0xac } },
            { 0x82c2, new byte[] { 0xe2, 0x80, 0x9a } },
            { 0x83c2, new byte[] { 0xc6, 0x92 } },
            { 0x84c2, new byte[] { 0xe2, 0x80, 0x9e } },
            { 0x85c2, new byte[] { 0xe2, 0x80, 0xa6 } },
            { 0x86c2, new byte[] { 0xe2, 0x80, 0xa0 } },
            { 0x87c2, new byte[] { 0xe2, 0x80, 0xa1 } },
            { 0x88c2, new byte[] { 0xcb, 0x86 } },
            { 0x89c2, new byte[] { 0xe2, 0x80, 0xb0 } },
            { 0x8ac2, new byte[] { 0xc5, 0xa0 } },
            { 0x8bc2, new byte[] { 0xe2, 0x80, 0xb9 } },
            { 0x8cc2, new byte[] { 0xc5, 0x92 } },
            { 0x8ec2, new byte[] { 0xc5, 0xbd } },
            { 0x91c2, new byte[] { 0xe2, 0x80, 0x98 } },
            { 0x92c2, new byte[] { 0xe2, 0x80, 0x99 } },
            { 0x93c2, new byte[] { 0xe2, 0x80, 0x9c } },
            { 0x94c2, new byte[] { 0xe2, 0x80, 0x9d } },
            { 0x95c2, new byte[] { 0xe2, 0x80, 0xa2 } },
            { 0x96c2, new byte[] { 0xe2, 0x80, 0x93 } },
            { 0x97c2, new byte[] { 0xe2, 0x80, 0x94 } },
            { 0x98c2, new byte[] { 0xcb, 0x9c } },
            { 0x99c2, new byte[] { 0xe2, 0x84, 0xa2 } },
            { 0x9ac2, new byte[] { 0xc5, 0xa1 } },
            { 0x9bc2, new byte[] { 0xe2, 0x80, 0xba } },
            { 0x9cc2, new byte[] { 0xc5, 0x93 } },
            { 0x9ec2, new byte[] { 0xc5, 0xbe } },
            { 0x9fc2, new byte[] { 0xc5, 0xb8 } }
        };

        private static readonly Dictionary<uint, byte[]> Utf8ToWin1252 = new Dictionary<uint, byte[]>
        {    
            { 0xac82e2, new byte[] { 0x80 } },
            { 0x9a80e2, new byte[] { 0x82 } },
            { 0x92c6, new byte[] { 0x83 } },
            { 0x9e80e2, new byte[] { 0x84 } },
            { 0xa680e2, new byte[] { 0x85 } },
            { 0xa080e2, new byte[] { 0x86 } },
            { 0xa180e2, new byte[] { 0x87 } },
            { 0x86cb, new byte[] { 0x88 } },
            { 0xb080e2, new byte[] { 0x89 } },
            { 0xa0c5, new byte[] { 0x8a } },
            { 0xb980e2, new byte[] { 0x8b } },
            { 0x92c5, new byte[] { 0x8c } },
            { 0xbdc5, new byte[] { 0x8e } },
            { 0x9880e2, new byte[] { 0x91 } },
            { 0x9980e2, new byte[] { 0x92 } },
            { 0x9c80e2, new byte[] { 0x93 } },
            { 0x9d80e2, new byte[] { 0x94 } },
            { 0xa280e2, new byte[] { 0x95 } },
            { 0x9380e2, new byte[] { 0x96 } },
            { 0x9480e2, new byte[] { 0x97 } },
            { 0x9ccb, new byte[] { 0x98 } },
            { 0xa284e2, new byte[] { 0x99 } },
            { 0xa1c5, new byte[] { 0x9a } },
            { 0xba80e2, new byte[] { 0x9b } },
            { 0x93c5, new byte[] { 0x9c } },
            { 0xbec5, new byte[] { 0x9e } },
            { 0xb8c5, new byte[] { 0x9f } }
        };

        public static string ToUtf8(string text)
        {
            return ProcessUtf8(text, ToUtf8);
        }

        /// <summary>
        /// This function leaves UTF8 characters alone, while converting almost all non-UTF8 to UTF8.
        ///
        /// It assumes that the encoding of the original string is either Windows-1252 or ISO 8859-1.
        ///
        /// It may fail to convert characters to UTF-8 if they fall into one of these scenarios:
        ///
        /// 1) when any of these characters:   ÀÁÂÃÄÅÆÇÈÉÊËÌÍÎÏÐÑÒÓÔÕÖ×ØÙÚÛÜÝÞß
        ///    are followed by any of these:  ("group B")
        ///                                    ¡¢£¤¥¦§¨©ª«¬­®¯°±²³´µ¶•¸¹º»¼½¾¿
        /// For example:   %ABREPRESENT%C9%BB. «REPRESENTÉ»
        /// The "«" (%AB) character will be converted, but the "É" followed by "»" (%C9%BB)
        /// is also a valid unicode character, and will be left unchanged.
        ///
        /// 2) when any of these: àáâãäåæçèéêëìíîï  are followed by TWO chars from group B,
        /// 3) when any of these: ðñòó  are followed by THREE chars from group B.
        /// </summary>
        /// <param name="text">Any text bytes</param>
        /// <returns>The same string, UTF8 encoded</returns>
        public static byte[] ToUtf8(IReadOnlyList<byte> text)
        {
            var max = text.Count;
            var ms = new MemoryStream(max);
            var i = 0;
            while (i < max)
            {
                var c1 = text[i];
                if (c1 >= 0xc0)
                {
                    // should be converted to UTF8, if it's not UTF8 already
                    var c2 = i + 1 >= max ? (byte)0x00 : text[i + 1];
                    var c3 = i + 2 >= max ? (byte)0x00 : text[i + 2];
                    var c4 = i + 3 >= max ? (byte)0x00 : text[i + 3];
                    if (c1 >= 0xc0 & c1 <= 0xdf)
                    {
                        // looks like 2 bytes UTF8
                        if (c2 >= 0x80 && c2 <= 0xbf)
                        {
                            // yeah, almost sure it's UTF8 already
                            ms.WriteByte(c1);
                            ms.WriteByte(c2);
                            i += 2;
                            continue;
                        }
                    }
                    else if (c1 >= 0xe0 & c1 <= 0xef)
                    {
                        // looks like 3 bytes UTF8
                        if (c2 >= 0x80 && c2 <= 0xbf && c3 >= 0x80 && c3 <= 0xbf)
                        {
                            // yeah, almost sure it's UTF8 already
                            ms.WriteByte(c1);
                            ms.WriteByte(c2);
                            ms.WriteByte(c3);
                            i += 3;
                            continue;
                        }
                    }
                    else if (c1 >= 0xf0 & c1 <= 0xf7)
                    {
                        // looks like 4 bytes UTF8
                        if (c2 >= 0x80 && c2 <= 0xbf && c3 >= 0x80 && c3 <= 0xbf && c4 >= 0x80 && c4 <= 0xbf)
                        {
                            // yeah, almost sure it's UTF8 already
                            ms.WriteByte(c1);
                            ms.WriteByte(c2);
                            ms.WriteByte(c3);
                            ms.WriteByte(c4);
                            i += 4;
                            continue;
                        }
                    }
                }
                else if ((c1 & 0xc0) == 0x80)
                {
                    // needs conversion
                    if (Win1252ToUtf8.TryGetValue(c1, out var win1252))
                    {
                        // found in Windows-1252 special cases
                        ms.Write(win1252, 0, win1252.Length);
                        i++;
                        continue;
                    }
                }
                else
                {
                    // it doesn't need conversion
                    ms.WriteByte(c1);
                    i++;
                    continue;
                }

                // doesn't look like UTF8, but should be converted
                var cc1 = (byte)((c1 / 64) | 0xc0);
                var cc2 = (byte)((c1 & 0x3f) | 0x80);
                ms.WriteByte(cc1);
                ms.WriteByte(cc2);
                i++;
            }
            
            return ms.ToArray();
        }

        /// <summary>
        /// If you received an UTF-8 string that was converted from Windows-1252 as it was ISO8859-1
        /// (ignoring Windows-1252 chars from 80 to 9F) use this function to fix it.
        /// See: http://en.wikipedia.org/wiki/Windows-1252
        /// </summary>
        /// <param name="text">Any text bytes</param>
        /// <returns>Fixed text bytes</returns>
        public static byte[] Utf8FixWin1252Chars(IReadOnlyList<byte> text)
        {
            return Replace(text, BrokenUtf8ToUtf8, 2, 2);
        }

        /// <summary>
        /// FixUTF8() fixes the double (or multiple) encoded UTF8 string that looks garbled
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static byte[] FixUtf8(IReadOnlyList<byte> text)
        {
            return ToUtf8(Replace(text, Utf8ToWin1252, 2, 3));
        }

        public static string FixUtf8(string text)
        {
            return ProcessUtf8(text, FixUtf8);
        }

        private static string ProcessUtf8(string text, Func<IReadOnlyList<byte>, byte[]> action)
        {
            var unicodeBytes = Encoding.Unicode.GetBytes(text);
            var utf8Bytes = Encoding.Convert(Encoding.Unicode, Encoding.UTF8, unicodeBytes);
            var converted = action(utf8Bytes);
            return Encoding.UTF8.GetString(converted, 0, converted.Length);
        }

        private static byte[] Replace(IReadOnlyList<byte> text, IDictionary<uint, byte[]> replacements, int minLen, int maxLen)
        {
            var i = 0;
            var max = text.Count;
            var ms = new MemoryStream(max);
            while (i < max)
            {
                var c1 = text[i];
                var isReplaced = false;
                for (var len = maxLen; len >= minLen; len--)
                {
                    var c2 = i + 1 >= max || len < 2 ? (byte)0x00 : text[i + 1];
                    var c3 = i + 2 >= max || len < 3 ? (byte)0x00 : text[i + 2];
                    var c4 = i + 3 >= max || len < 4 ? (byte)0x00 : text[i + 3];
                    var index = (uint)(c1 | c2 << 8 | c3 << 16 | c4 << 24);
                    if (replacements.TryGetValue(index, out var replacement))
                    {
                        ms.Write(replacement, 0, replacement.Length);
                        i += len - 1;
                        isReplaced = true;
                        break;
                    }
                }

                if (!isReplaced)
                {
                    ms.WriteByte(c1);
                }

                i++;
            }

            return ms.ToArray();
        }
    }
}
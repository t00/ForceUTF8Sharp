# ForceUTF8Sharp
C# netstandard 1.0 library which fixes malformed UTF8 strings

## Purpose
Library will process byte array containing raw text and validate UTF8 encoding marks.
If the 2, 3 or 4 byte UTF8 sequence is recognized, it will be copied without change,
but characters which might have been left without correct UTF8 prefixes will be escaped.

## Usage
To encode given byte array:

```
var convertedBytes = ForceUtf8.FixUtf8(textBytes);
```

## Credits
Library is based on php implementation by Sebastián Grignoli available on:
https://github.com/neitanod/forceutf8

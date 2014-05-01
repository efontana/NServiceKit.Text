using System;
using System.Collections.Generic;
using System.IO;

namespace NServiceKit.Text
{
    /// <summary>A stream extensions.</summary>
	public static class StreamExtensions
	{
        /// <summary>A Stream extension method that writes to.</summary>
        /// <param name="inStream"> The inStream to act on.</param>
        /// <param name="outStream">Stream to write data to.</param>
		public static void WriteTo(this Stream inStream, Stream outStream)
		{
			var memoryStream = inStream as MemoryStream;
			if (memoryStream != null)
			{
				memoryStream.WriteTo(outStream);
				return;
			}

			var data = new byte[4096];
			int bytesRead;

			while ((bytesRead = inStream.Read(data, 0, data.Length)) > 0)
			{
				outStream.Write(data, 0, bytesRead);
			}
		}

        /// <summary>Enumerates read lines in this collection.</summary>
        /// <exception cref="ArgumentNullException">Thrown when one or more required arguments are null.</exception>
        /// <param name="reader">The reader to act on.</param>
        /// <returns>
        /// An enumerator that allows foreach to be used to process read lines in this collection.
        /// </returns>
		public static IEnumerable<string> ReadLines(this StreamReader reader)
		{
			if (reader == null)
				throw new ArgumentNullException("reader");

			string line;
			while ((line = reader.ReadLine()) != null)
			{
				yield return line;
			}
		}

        /// <summary>
        /// @jonskeet: Collection of utility methods which operate on streams. r285, February 26th 2009:
        /// http://www.yoda.arachsys.com/csharp/miscutil/.
        /// </summary>
		const int DefaultBufferSize = 8 * 1024;

        /// <summary>Reads the given stream up to the end, returning the data as a byte array.</summary>
        /// <param name="input">The input to act on.</param>
        /// <returns>An array of byte.</returns>
		public static byte[] ReadFully(this Stream input)
		{
			return ReadFully(input, DefaultBufferSize);
		}

        /// <summary>
        /// Reads the given stream up to the end, returning the data as a byte array, using the given
        /// buffer size.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when one or more arguments are outside the
        /// required range.</exception>
        /// <param name="input">     The input to act on.</param>
        /// <param name="bufferSize">Size of the buffer.</param>
        /// <returns>An array of byte.</returns>
		public static byte[] ReadFully(this Stream input, int bufferSize)
		{
			if (bufferSize < 1)
			{
				throw new ArgumentOutOfRangeException("bufferSize");
			}
			return ReadFully(input, new byte[bufferSize]);
		}

        /// <summary>
        /// Reads the given stream up to the end, returning the data as a byte array, using the given
        /// buffer for transferring data. Note that the current contents of the buffer is ignored, so the
        /// buffer needn't be cleared beforehand.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown when one or more required arguments are null.</exception>
        /// <exception cref="ArgumentException">    Thrown when one or more arguments have unsupported or
        /// illegal values.</exception>
        /// <param name="input"> The input to act on.</param>
        /// <param name="buffer">The buffer.</param>
        /// <returns>An array of byte.</returns>
		public static byte[] ReadFully(this Stream input, byte[] buffer)
		{
			if (buffer == null)
			{
				throw new ArgumentNullException("buffer");
			}
			if (input == null)
			{
				throw new ArgumentNullException("input");
			}
			if (buffer.Length == 0)
			{
				throw new ArgumentException("Buffer has length of 0");
			}
			// We could do all our own work here, but using MemoryStream is easier
			// and likely to be just as efficient.
			using (var tempStream = new MemoryStream())
			{
				CopyTo(input, tempStream, buffer);
				// No need to copy the buffer if it's the right size
#if !NETFX_CORE
				if (tempStream.Length == tempStream.GetBuffer().Length)
				{
					return tempStream.GetBuffer();
				}
#endif
				// Okay, make a copy that's the right size
				return tempStream.ToArray();
			}
		}

        /// <summary>Copies all the data from one stream into another.</summary>
        /// <param name="input"> The input to act on.</param>
        /// <param name="output">The output.</param>
		public static void CopyTo(this Stream input, Stream output)
		{
			CopyTo(input, output, DefaultBufferSize);
		}

        /// <summary>
        /// Copies all the data from one stream into another, using a buffer of the given size.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when one or more arguments are outside the
        /// required range.</exception>
        /// <param name="input">     The input to act on.</param>
        /// <param name="output">    The output.</param>
        /// <param name="bufferSize">Size of the buffer.</param>
		public static void CopyTo(this Stream input, Stream output, int bufferSize)
		{
			if (bufferSize < 1)
			{
				throw new ArgumentOutOfRangeException("bufferSize");
			}
			CopyTo(input, output, new byte[bufferSize]);
		}

        /// <summary>
        /// Copies all the data from one stream into another, using the given buffer for transferring
        /// data. Note that the current contents of the buffer is ignored, so the buffer needn't be
        /// cleared beforehand.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown when one or more required arguments are null.</exception>
        /// <exception cref="ArgumentException">    Thrown when one or more arguments have unsupported or
        /// illegal values.</exception>
        /// <param name="input"> The input to act on.</param>
        /// <param name="output">The output.</param>
        /// <param name="buffer">The buffer.</param>
		public static void CopyTo(this Stream input, Stream output, byte[] buffer)
		{
			if (buffer == null)
			{
				throw new ArgumentNullException("buffer");
			}
			if (input == null)
			{
				throw new ArgumentNullException("input");
			}
			if (output == null)
			{
				throw new ArgumentNullException("output");
			}
			if (buffer.Length == 0)
			{
				throw new ArgumentException("Buffer has length of 0");
			}
			int read;
			while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
			{
				output.Write(buffer, 0, read);
			}
		}

        /// <summary>
        /// Reads exactly the given number of bytes from the specified stream. If the end of the stream
        /// is reached before the specified amount of data is read, an exception is thrown.
        /// </summary>
        /// <param name="input">      The input to act on.</param>
        /// <param name="bytesToRead">The bytes to read.</param>
        /// <returns>An array of byte.</returns>
		public static byte[] ReadExactly(this Stream input, int bytesToRead)
		{
			return ReadExactly(input, new byte[bytesToRead]);
		}

        /// <summary>Reads into a buffer, filling it completely.</summary>
        /// <param name="input"> The input to act on.</param>
        /// <param name="buffer">The buffer.</param>
        /// <returns>An array of byte.</returns>
		public static byte[] ReadExactly(this Stream input, byte[] buffer)
		{
			return ReadExactly(input, buffer, buffer.Length);
		}

        /// <summary>
        /// Reads exactly the given number of bytes from the specified stream, into the given buffer,
        /// starting at position 0 of the array.
        /// </summary>
        /// <param name="input">      The input to act on.</param>
        /// <param name="buffer">     The buffer.</param>
        /// <param name="bytesToRead">The bytes to read.</param>
        /// <returns>An array of byte.</returns>
		public static byte[] ReadExactly(this Stream input, byte[] buffer, int bytesToRead)
		{
			return ReadExactly(input, buffer, 0, bytesToRead);
		}

        /// <summary>
        /// Reads exactly the given number of bytes from the specified stream, into the given buffer,
        /// starting at position 0 of the array.
        /// </summary>
        /// <exception cref="ArgumentNullException">      Thrown when one or more required arguments are
        /// null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when one or more arguments are outside the
        /// required range.</exception>
        /// <param name="input">      The input to act on.</param>
        /// <param name="buffer">     The buffer.</param>
        /// <param name="startIndex"> The start index.</param>
        /// <param name="bytesToRead">The bytes to read.</param>
        /// <returns>An array of byte.</returns>
		public static byte[] ReadExactly(this Stream input, byte[] buffer, int startIndex, int bytesToRead)
		{
			if (input == null)
			{
				throw new ArgumentNullException("input");
			}

			if (buffer == null)
			{
				throw new ArgumentNullException("buffer");
			}

			if (startIndex < 0 || startIndex >= buffer.Length)
			{
				throw new ArgumentOutOfRangeException("startIndex");
			}

			if (bytesToRead < 1 || startIndex + bytesToRead > buffer.Length)
			{
				throw new ArgumentOutOfRangeException("bytesToRead");
			}

			return ReadExactlyFast(input, buffer, startIndex, bytesToRead);
		}

        /// <summary>Same as ReadExactly, but without the argument checks.</summary>
        /// <exception cref="EndOfStreamException">Thrown when the end of the stream was unexpectedly
        /// reached.</exception>
        /// <param name="fromStream">  from stream.</param>
        /// <param name="intoBuffer">  Buffer for into data.</param>
        /// <param name="startAtIndex">The start at index.</param>
        /// <param name="bytesToRead"> The bytes to read.</param>
        /// <returns>An array of byte.</returns>
		private static byte[] ReadExactlyFast(Stream fromStream, byte[] intoBuffer, int startAtIndex, int bytesToRead)
		{
			var index = 0;
			while (index < bytesToRead)
			{
				var read = fromStream.Read(intoBuffer, startAtIndex + index, bytesToRead - index);
				if (read == 0)
				{
					throw new EndOfStreamException
						(String.Format("End of stream reached with {0} byte{1} left to read.",
						               bytesToRead - index,
						               bytesToRead - index == 1 ? "s" : ""));
				}
				index += read;
			}
			return intoBuffer;
		}
	}
}
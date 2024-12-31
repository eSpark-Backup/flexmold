using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace FlexMold.Utility
{
    internal static class MixUtil
    {
        public static async Task<string> ReadTextFromFileAsync(string filePath)
        {
            var sourceStream =
               new FileStream(
                   filePath,
                   FileMode.Open, FileAccess.Read, FileShare.Read,
                   bufferSize: 4096, useAsync: true);

            var sb = new StringBuilder();

            byte[] buffer = new byte[0x1000];
            int numRead;
            while ((numRead = await sourceStream.ReadAsync(buffer, 0, buffer.Length)) != 0)
            {
                string text = Encoding.ASCII.GetString(buffer, 0, numRead);
                sb.Append(text);
            }

            return sb.ToString();
        }

        public static T CastToStruct<T>(this byte[] data) where T : struct
        {
            var pData = GCHandle.Alloc(data, GCHandleType.Pinned);
            var result = (T)Marshal.PtrToStructure(pData.AddrOfPinnedObject(), typeof(T));
            pData.Free();
            return result;
        }

        public static int[] FindAllIndexof<T>(this IEnumerable<T> values, T val)
        {
            return values.Select((b, i) => Equals(b, val) ? i : -1).Where(i => i != -1).ToArray();
        }

        public static byte[] CastToArray<T>(this T data) where T : struct
        {
            var result = new byte[Marshal.SizeOf(typeof(T))];
            var pResult = GCHandle.Alloc(result, GCHandleType.Pinned);
            Marshal.StructureToPtr(data, pResult.AddrOfPinnedObject(), true);
            pResult.Free();
            return result;
        }
    }
}
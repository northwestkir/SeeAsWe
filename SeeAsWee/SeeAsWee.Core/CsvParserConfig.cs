using System.Buffers;
using System.Text;

namespace SeeAsWee.Core
{
	public class CsvParserConfig
	{
		public ArrayPool<byte> ArrayPool { get; set; } = ArrayPool<byte>.Shared;
		public bool HasHeader { get; set; } = true;
		public int RentBytesBuffer { get; set; } = 1024;
		public Encoding Encoding { get; set; } = Encoding.UTF8;
		public char Separator { get; set; } = ',';
		public bool SetMembersFromHeader { get; set; }
	}
}
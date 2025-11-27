// Error or warning message for T59 programs
// No error/warning level for now
//
// 2025-11-27   PV

namespace T59v5;

internal sealed class T59Message
{
    public required string Message { get; set; }
    public required L2StatementBase Statement { get; set; }
    public L1Token? Token { get; set; }
}

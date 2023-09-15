///
/// Author:Ruipeng Li, Model for Invest Cloud Api
///

namespace MatrixMultiplication
{
    internal class InvestCloudApiResult<T>
    {
        public bool Success { get; set; }
        public string? Cause { get; set; }
        public T? Value { get; set; }
    }
}

using Flurl;
using Flurl.Http;
using MathNet.Numerics.LinearAlgebra.Double;
using Matrixden.Utils.Extensions;
using MatrixMultiplication;
using System.Diagnostics;
using System.Text.RegularExpressions;

Console.WriteLine("*********************************************************");
Console.WriteLine("*                                                       *");
Console.WriteLine("*           Server Side Test - C# Ruipeng Li.           *");
Console.WriteLine("*                                                       *");
Console.WriteLine("*********************************************************");

Console.WriteLine("Start");

var sw = new Stopwatch();
var basUrl = "https://recruitment-test.investcloud.com/api/numbers";
int size = 1000;

//1. Init matrix size
//GET api/numbers/init/{size}
Console.WriteLine("\r\n1. Init matrix size");
sw.Start();
var ar1 = await basUrl.AppendPathSegments("init", size).GetJsonAsync<InvestCloudApiResult<int>>();
if (!ar1.Success)
{
    Console.WriteLine(ar1.Cause);
    return;
}
Console.WriteLine($"It takes {sw.ElapsedMilliseconds} ms to initialize 2 datasets.");

//2. Fill matrix row by row
//GET api/numbers/{dataset}/{type}/{idx}
//var mA = Matrix.Build.DenseIdentity(size);
//var mB = Matrix.Build.DenseIdentity(size);
Console.WriteLine("\r\n2. Fill dataset by row");

Matrix A, B;

//2.1 Fill matrix A
Console.WriteLine($"2.1 Fill dataset {nameof(A)}");
sw.Restart();
var a = new double[size][];
InvestCloudApiResult<double[]> ar2;

Parallel.For(0, size, idx =>
{
    ar2 = basUrl.AppendPathSegments(nameof(A), "row", idx).GetJsonAsync<InvestCloudApiResult<double[]>>().Result;
    if (!ar2.Success)
    {
        Console.WriteLine(ar2.Cause);
        return;
	}

	a[idx] = ar2.Value;
});

A = DenseMatrix.OfRowArrays(a);


Console.WriteLine($"Dataset {nameof(A)}");
Console.WriteLine(A.ToMatrixString());

Console.WriteLine($"It takes {sw.ElapsedMilliseconds} ms to fill dataset {nameof(A)}.");

//2.2 Fill matrix B
Console.WriteLine($"\r\n2.2 Fill dataset {nameof(B)}");
sw.Restart();
Parallel.For(0, size, idx =>
{
    ar2 = basUrl.AppendPathSegments(nameof(B), "row", idx).GetJsonAsync<InvestCloudApiResult<double[]>>().Result;
    if (!ar2.Success)
    {
        Console.WriteLine(ar2.Cause);
        return;
    }

    a[idx] = ar2.Value;
});
B = DenseMatrix.OfRowArrays(a);

Console.WriteLine($"Dataset {nameof(B)}");
Console.WriteLine(B.ToMatrixString());
Console.WriteLine($"It takes {sw.ElapsedMilliseconds}ms to fill dataset {nameof(B)}.");

//3. Calculate the product of two matrices, then upload its' MD5 value for validating.
Console.WriteLine("\r\n3. Calculate the product of two datasets, then upload its' MD5 value for validating.");
//POST api/numbers/validate
//3.1 Cal the product
Console.WriteLine("3.1 Perform Calculation");
sw.Restart();

var mC = A * B;


Console.WriteLine($"Matrix {nameof(mC)}");
Console.WriteLine(mC.ToMatrixString());
Console.WriteLine($"\r\nIt takes {sw.ElapsedMilliseconds} ms or {sw.ElapsedMilliseconds / 1000} s to cal the production of 2 datasets.");

//3.2 Joined the elements' into a string, and validate its MD5 value.
Console.WriteLine("\r\n3.2 Joined the elements' into a string, and validate its MD5 value.");
sw.Restart();
//var str = mC.ToMatrixString();
var str = new Regex("\\s+").Replace(mC.ToMatrixString(), "");
var md5 = str.MD5Value();

Console.WriteLine($"Result str: {str}");
Console.WriteLine($"MD5 val: {md5}");
Console.WriteLine($"It takes {sw.ElapsedMilliseconds}ms to generate the MD5 value of the joined string.");

sw.Restart();
var ar3 = await basUrl.AppendPathSegments("validate").PostJsonAsync(str).ReceiveJson<InvestCloudApiResult<string>>();
if (!ar3.Success)
{
    Console.WriteLine(ar3.Cause);
    return;
}

Console.WriteLine(ar3.Value);
Console.WriteLine($"It takes {sw.ElapsedMilliseconds}ms to validate the calculated matrix.");

if(ar3.Value == "Alas it didn't work")
Console.WriteLine("\r\n\r\nIt seems like the calculation is correct but the matrix format is wrong.");

Console.ReadLine();
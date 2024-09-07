using System.Text;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;

namespace Benchmark;

[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
[CategoriesColumn]
[MemoryDiagnoser]
[SimpleJob(iterationCount: 20)]
public class Benchmark
{
    private MemoryEfficientBooleanArray32 _arr32 = default!;
    private bool[] _bool = default!;

    [ParamsSource(nameof(GenerateLarge))]
    public int Size { get; set; }

    [GlobalSetup]
    public void Initialize()
    {
        _bool = new bool[Size];
        _arr32 = new MemoryEfficientBooleanArray32(Size);
    }

    [BenchmarkCategory("Create"), Benchmark(Baseline = true)]
    public bool[] CreateBool() => new bool[Size];

    [BenchmarkCategory("Create"), Benchmark]
    public MemoryEfficientBooleanArray32 Create32() => new(Size);

    [BenchmarkCategory("Set"), Benchmark(Baseline = true)]
    public bool[] SetBool()
    {
        for (var i = 0; i < Size; i++) _bool[i] = i % 2 == 0;
        return _bool;
    }

    [BenchmarkCategory("Set"), Benchmark]
    public MemoryEfficientBooleanArray32 Set32()
    {
        for (var i = 0; i < Size; i++) _arr32[i] = i % 2 == 0;
        return _arr32;
    }

    [BenchmarkCategory("Get"), Benchmark(Baseline = true)]
    public int GetBool()
    {
        var count = 0;
        for (var i = 0; i < Size; i++) if (_bool[i]) count++;
        return count;
    }

    [BenchmarkCategory("Get"), Benchmark]
    public int Get32()
    {
        var count = 0;
        for (var i = 0; i < Size; i++) if (_arr32[i]) count++;
        return count;
    }

    public static IEnumerable<int> GenerateLarge()
    {
        for (int i = 10; i < 100; i += 10) yield return i;
        for (int i = 100; i < 1_000; i += 100) yield return i;
        for (int i = 1_000; i < 10_000; i += 1000) yield return i;
        for (int i = 10_000; i < 100_000; i += 10_000) yield return i;
        for (int i = 100_000; i <= 1_000_000; i += 100_000) yield return i;
    }
}

public struct MemoryEfficientBooleanArray32(int length)
{
    private const int bitsPerBucket = 32;
    private readonly int[] _array = new int[length / bitsPerBucket + 1];

    public readonly bool this[int index]
    {
        get => (_array[index / bitsPerBucket] & 1 << index) != 0;
        set
        {
            var (bucketNumber, bitNumber) = (index / bitsPerBucket, index % bitsPerBucket);

            if (value) _array[bucketNumber] |= 1 << bitNumber;
            else _array[bucketNumber] &= ~(1 << bitNumber);
        }
    }

    public override readonly string ToString()
    {
        var sb = new StringBuilder();
        
        for (int i = 0; i < _array.Length; i++)
        {
            sb.Append(Convert.ToString(_array[i], 2).PadLeft(bitsPerBucket, '0').Reverse().ToArray());
            sb.Append(' ');
        }

        return sb.ToString();
    }
}
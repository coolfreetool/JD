using System;
using NUnit.Framework;

public static class AssertExtensions
{
    public static void AreEqual(double[,] expected, double[,] actual, double tolerance = 1e-10)
    {
        // Check if dimensions match
        Assert.AreEqual(expected.GetLength(0), actual.GetLength(0), "Row count mismatch");
        Assert.AreEqual(expected.GetLength(1), actual.GetLength(1), "Column count mismatch");

        // Compare each element
        for (int i = 0; i < expected.GetLength(0); i++)
        {
            for (int j = 0; j < expected.GetLength(1); j++)
            {
                Assert.That(Math.Abs(expected[i, j] - actual[i, j]), Is.LessThanOrEqualTo(tolerance),
                            $"Element at ({i}, {j}) differs. Expected: {expected[i, j]}, Actual: {actual[i, j]}");
            }
        }
    }

    public static void AreEqual(double?[,] expected, double?[,] actual, double tolerance = 1e-10)
    {
        // Check if dimensions match
        Assert.AreEqual(expected.GetLength(0), actual.GetLength(0), "Row count mismatch");
        Assert.AreEqual(expected.GetLength(1), actual.GetLength(1), "Column count mismatch");

        // Compare each element
        for (int i = 0; i < expected.GetLength(0); i++)
        {
            for (int j = 0; j < expected.GetLength(1); j++)
            {
                if ((expected[i, j] == null) || (actual[i, j] != null)) {
                    Assert.AreEqual(expected[i, j], actual[i, j]);
                }

                Assert.That(Math.Abs((double)expected[i, j] - (double)actual[i, j]), Is.LessThanOrEqualTo(tolerance),
                            $"Element at ({i}, {j}) differs. Expected: {expected[i, j]}, Actual: {actual[i, j]}");
            }
        }
    }
}

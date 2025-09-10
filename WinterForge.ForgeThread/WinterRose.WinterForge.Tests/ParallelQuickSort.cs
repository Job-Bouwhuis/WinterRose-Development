using WinterRose.ForgeThread;

namespace WinterRose.ForgeThreads.Tests;

public static class ParallelQuickSort<T> where T : IComparable<T>
{
    private const int MIN_PARALLEL_SIZE = 10_000; // tweak this based on your dataset size

    public static void Sort(T[] array, PooledThreadLoom loom)
    {
        var done = new CountdownEvent(1);
        QuickSort(array, 0, array.Length - 1, loom, done);
        done.Wait(); // wait until all parallel tasks finish
    }

    private static void QuickSort(T[] array, int left, int right, PooledThreadLoom loom, CountdownEvent done)
    {
        if (left >= right)
        {
            done.Signal();
            return;
        }

        int pivot = Partition(array, left, right);
        bool runParallel = (right - left) > MIN_PARALLEL_SIZE;

        if (runParallel)
        {
            done.AddCount(2);
            loom.Schedule(() => QuickSort(array, left, pivot - 1, loom, done));
            loom.Schedule(() => QuickSort(array, pivot + 1, right, loom, done));
        }
        else
        {
            QuickSortSequential(array, left, pivot - 1);
           QuickSortSequential(array, pivot + 1, right);
        }

        done.Signal();
    }

    private static void QuickSortSequential(T[] array, int left, int right)
    {
        if (left >= right) return;

        int pivot = Partition(array, left, right);
        QuickSortSequential(array, left, pivot - 1);
        QuickSortSequential(array, pivot + 1, right);
    }

    private static int Partition(T[] array, int left, int right)
    {
        T pivot = MedianPivot(array, left, right);
        int i = left - 1;
        int j = right + 1;

        while (true)
        {
            do { i++; } while (array[i].CompareTo(pivot) < 0);
            do { j--; } while (array[j].CompareTo(pivot) > 0);

            if (i >= j) return j;

            (array[i], array[j]) = (array[j], array[i]);
        }
    }

    private static T MedianPivot(T[] array, int left, int right)
    {
        int mid = left + (right - left) / 2;
        T a = array[left], b = array[mid], c = array[right];

        if (a.CompareTo(b) > 0) (a, b) = (b, a);
        if (a.CompareTo(c) > 0) (a, c) = (c, a);
        if (b.CompareTo(c) > 0) (b, c) = (c, b);
        return b;
    }

}

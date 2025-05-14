namespace FileChangeTracker.detecting
{
    public record ChunkDifference(int ChunkIndex, byte[]? OldHash, byte[]? NewHash);
}

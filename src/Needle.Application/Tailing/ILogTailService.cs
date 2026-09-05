namespace Needle.Application.Tailing;

public interface ILogTailService
{
    ValueTask<TailUpdateResult> PollAsync(CancellationToken cancellationToken = default);
}

public abstract class BaseInitHandler
{
    private BaseInitHandler _next;

    public BaseInitHandler(BaseInitHandler next = null)
    {
        _next = next;
    }

    public void SetNext(BaseInitHandler next)
    {
        _next = next;
    }

    public void Handle(InitContext context)
    {
        if (Init(context))
        {
            _next?.Handle(context);
            return;
        }

        throw new System.Exception($"Init failed at {GetType().Name}");
    }

    protected abstract bool Init(InitContext context);
}

public struct InitContext
{
    private GlobalFlags _globalFlags;
    private TBS_InitConfigSO _initConfig;

    public GlobalFlags GF => _globalFlags;
    public TBS_InitConfigSO Config => _initConfig;

    public InitContext(GlobalFlags globalFlags, TBS_InitConfigSO initConfig)
    {
        _globalFlags = globalFlags;
        _initConfig = initConfig;
    }
}
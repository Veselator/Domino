public class PlayersManagerInitHandler : BaseInitHandler
{
    public PlayersManagerInitHandler(BaseInitHandler next = null) : base(next) { }

    protected override bool Init(InitContext context)
    {
        if (TBS_PlayersManager.Instance != null)
        {
            TBS_PlayersManager.Instance.Init(context.GF, context.Config);
            return true;
        }

        return false;
    }
}

public class OrderManagerInitHandler : BaseInitHandler
{
    public OrderManagerInitHandler(BaseInitHandler next = null) : base(next) { }

    protected override bool Init(InitContext context)
    {
        if (TBS_OrderManager.Instance != null)
        {
            TBS_OrderManager.Instance.Init(context.GF, context.Config);
            return true;
        }

        return false;
    }
}

public class RulesManagerInitHandler : BaseInitHandler
{
    public RulesManagerInitHandler(BaseInitHandler next = null) : base(next) { }

    protected override bool Init(InitContext context)
    {
        if (TBS_RulesManager.Instance != null)
        {
            TBS_RulesManager.Instance.Init(context.GF, context.Config);
            return true;
        }

        return false;
    }
}

public class MapInitHandler : BaseInitHandler
{
    public MapInitHandler(BaseInitHandler next = null) : base(next) { }

    protected override bool Init(InitContext context)
    {
        if (TBS_BaseMap.Instance != null)
        {
            TBS_BaseMap.Instance.Init(context.Config);
            return true;
        }

        return false;
    }
}

public class BeforeTurnStartInitHandler : BaseInitHandler
{
    public BeforeTurnStartInitHandler(BaseInitHandler next = null) : base(next) { }

    protected override bool Init(InitContext context)
    {
        if (TBS_BeforeTurnStartHandler.Instance != null)
        {
            TBS_BeforeTurnStartHandler.Instance.Init(context.GF);
            return true;
        }

        return false;
    }
}

public class BeforeTurnEndInitHandler : BaseInitHandler
{
    public BeforeTurnEndInitHandler(BaseInitHandler next = null) : base(next) { }

    protected override bool Init(InitContext context)
    {
        if (TBS_BeforeTurnEndHandler.Instance != null)
        {
            TBS_BeforeTurnEndHandler.Instance.Init(context.GF);
            return true;
        }

        return false;
    }
}

public class BeforeGameStartInitHandler : BaseInitHandler
{
    public BeforeGameStartInitHandler(BaseInitHandler next = null) : base(next) { }

    protected override bool Init(InitContext context)
    {
        if (TBS_BeforeGameStartHandler.Instance != null)
        {
            TBS_BeforeGameStartHandler.Instance.Init(context.GF);
            return true;
        }

        return false;
    }
}

public class RoundEndInitHandler : BaseInitHandler
{
    public RoundEndInitHandler(BaseInitHandler next = null) : base(next) { }

    protected override bool Init(InitContext context)
    {
        if (TBS_RoundEndHandler.Instance != null)
        {
            TBS_RoundEndHandler.Instance.Init(context.GF);
            return true;
        }

        return false;
    }
}

public class BeforeGameEndInitHandler : BaseInitHandler
{
    public BeforeGameEndInitHandler(BaseInitHandler next = null) : base(next) { }

    protected override bool Init(InitContext context)
    {
        if (TBS_BeforeGameEndHandler.Instance != null)
        {
            TBS_BeforeGameEndHandler.Instance.Init(context.GF);
            return true;
        }

        return false;
    }
}

public class PredictorInitHandler : BaseInitHandler
{
    public PredictorInitHandler(BaseInitHandler next = null) : base(next) { }

    protected override bool Init(InitContext context)
    {
        if (TBS_Predictor.Instance != null)
        {
            TBS_Predictor.Instance.Init();
            return true;
        }

        return false;
    }
}

public class TurnsManagerInitHandler : BaseInitHandler
{
    public TurnsManagerInitHandler(BaseInitHandler next = null) : base(next) { }

    protected override bool Init(InitContext context)
    {
        if (TBS_TurnsManager.Instance != null)
        {
            TBS_TurnsManager.Instance.Init(context.GF, context.Config);
            return true;
        }

        return false;
    }
}
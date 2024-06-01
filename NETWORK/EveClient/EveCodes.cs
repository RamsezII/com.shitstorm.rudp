namespace _RUDP_
{
    public enum EveCodes : byte
    {
        _none_,
        GetPublicEnd,
        ListHosts,
        AddHost,
        JoinHost,
#if UNITY_EDITOR
        Test,
#endif
        _last_,
    }
}
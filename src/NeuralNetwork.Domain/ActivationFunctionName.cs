namespace Common.Domain
{
    public enum ActivationFunctionName
    {
        Linear,
        Sigmoid,
        TanH,
        ArcTan
    }

    public enum ParamsInitMethod
    {
        DefaultNormalDist, NormalDist, Xavier, NguyenWidrow, SqrMUniform
    }
}
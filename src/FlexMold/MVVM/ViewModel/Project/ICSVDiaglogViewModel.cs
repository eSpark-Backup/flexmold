namespace FlexMold.MVVM.ViewModel.Project
{
    public interface ICSVDiaglogViewModel
    {
        int MotorDirPValue { get; set; }
        float MotorRampDelayValue { get; set; }
        float MotorTime { get; set; }
        float MaxDisplacement { get; set; }
        float RecommendedRPM { get; set; }
        float MaxRPM { get; set; }
    }
}

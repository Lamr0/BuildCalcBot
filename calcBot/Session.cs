namespace calcBot;

public enum Step { None, WaitLength, WaitHeight }


public class Session
{
    public Step Step { get; set; } = Step.None;
    public Wall Wall { get; set; }
    public double Length { get; set; }
}   
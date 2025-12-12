public class RuneBar : Bar
{
    public override void UpdateBar()
    {
        _bar.value = _currentValue/_maxValue;
    }
}

using Microsoft.Extensions.Options;

namespace ta.UIKit.Inputs;

public class InputManagerOptions : IOptions<InputManagerOptions>
{
    public int FramesBeforeEcho { get; set; } = 10;

    public bool ShouldProduceEchoForKeyboard { get; set; } = false;

    public bool ShouldProduceEchoForMouseButton { get; set; } = false;

    public bool ShouldProduceEchoForJoystickButton { get; set; } = false;

    InputManagerOptions IOptions<InputManagerOptions>.Value => this;
}

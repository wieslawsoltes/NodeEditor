namespace NodeEditor.Model;

public static class ConnectorExtensions
{
    public static void GetControlPoints(
        this IConnector connector,
        ConnectorOrientation orientation, 
        double offset, 
        PinAlignment p1A, 
        PinAlignment p2A, 
        ref double p1X, 
        ref double p1Y, 
        ref double p2X, 
        ref double p2Y)
    {
        switch (orientation)
        {
            case ConnectorOrientation.Auto:
                switch (p1A)
                {
                    case PinAlignment.None:
                    {
                        switch (p2A)
                        {
                            case PinAlignment.None:
                                break;
                            case PinAlignment.Left:
                                p2X -= offset;
                                p1X += offset;
                                break;
                            case PinAlignment.Right:
                                p2X += offset;
                                p1X -= offset;
                                break;
                            case PinAlignment.Top:
                                p2Y -= offset;
                                p1Y += offset;
                                break;
                            case PinAlignment.Bottom:
                                p2Y += offset;
                                p1Y -= offset;
                                break;
                        }
                    }
                        break;
                    case PinAlignment.Left:
                        switch (p2A)
                        {
                            case PinAlignment.None:
                                p1X -= offset;
                                p2X += offset;
                                break;
                            case PinAlignment.Left:
                                p1X -= offset;
                                p2X -= offset;
                                break;
                            case PinAlignment.Right:
                                p1X -= offset;
                                p2X += offset;
                                break;
                            case PinAlignment.Top:
                                p1X -= offset;
                                break;
                            case PinAlignment.Bottom:
                                p2Y += offset;
                                break;
                        }
                        break;
                    case PinAlignment.Right:
                        switch (p2A)
                        {
                            case PinAlignment.None:
                                p1X += offset;
                                p2X -= offset;
                                break;
                            case PinAlignment.Left:
                                p1X += offset;
                                p2X -= offset;
                                break;
                            case PinAlignment.Right:
                                p1X += offset;
                                p2X += offset;
                                break;
                            case PinAlignment.Top:
                                p1X += offset;
                                break;
                            case PinAlignment.Bottom:
                                p2Y += offset;
                                break;
                        }
                        break;
                    case PinAlignment.Top:
                        switch (p2A)
                        {
                            case PinAlignment.None:
                                p1Y -= offset;
                                p2Y += offset;
                                break;
                            case PinAlignment.Left:
                                p2X -= offset;
                                break;
                            case PinAlignment.Right:
                                p2X += offset;
                                break;
                            case PinAlignment.Top:
                                p1Y -= offset;
                                p2Y -= offset;
                                break;
                            case PinAlignment.Bottom:
                                p1Y -= offset;
                                p2Y += offset;
                                break;
                        }
                        break;
                    case PinAlignment.Bottom:
                        switch (p2A)
                        {
                            case PinAlignment.None:
                                p1Y += offset;
                                p2Y -= offset;
                                break;
                            case PinAlignment.Left:
                                p1Y += offset;
                                break;
                            case PinAlignment.Right:
                                p1Y += offset;
                                break;
                            case PinAlignment.Top:
                                p1Y += offset;
                                p2Y -= offset;
                                break;
                            case PinAlignment.Bottom:
                                p1Y += offset;
                                p2Y += offset;
                                break;
                        }
                        break;
                }

                break;
            case ConnectorOrientation.Horizontal:
                p1X += offset;
                p2X -= offset;
                break;
            case ConnectorOrientation.Vertical:
                p1Y += offset;
                p2Y -= offset;
                break;
        }
    }
}
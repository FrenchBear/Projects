﻿// MaxtrixAnimation class
// Support smooth matrix animation, missing in WPF
// From http://pwlodek.blogspot.fr/2010/12/matrixanimation-for-wpf.html
//
// 2017-07-25   PV
// 2021-11-13   PV      Net6 C#10
// 2024-11-15	PV		Net9 C#13

using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Bonza.Editor.Support;

public class MatrixAnimation: MatrixAnimationBase
{
    public Matrix? From
    {
        set => SetValue(FromProperty, value);
        get => (Matrix)GetValue(FromProperty);
    }

    internal static DependencyProperty FromProperty =
        DependencyProperty.Register("From", typeof(Matrix?), typeof(MatrixAnimation),
            new PropertyMetadata(null));

    public Matrix? To
    {
        set => SetValue(ToProperty, value);
        get => (Matrix)GetValue(ToProperty);
    }

    internal static DependencyProperty ToProperty =
        DependencyProperty.Register("To", typeof(Matrix?), typeof(MatrixAnimation),
            new PropertyMetadata(null));

    public IEasingFunction EasingFunction
    {
        get => (IEasingFunction)GetValue(EasingFunctionProperty);
        set => SetValue(EasingFunctionProperty, value);
    }

    public static readonly DependencyProperty EasingFunctionProperty =
        DependencyProperty.Register("EasingFunction", typeof(IEasingFunction), typeof(MatrixAnimation),
            new UIPropertyMetadata(null));

    public MatrixAnimation()
    {
    }

    public MatrixAnimation(Matrix toValue, Duration duration)
    {
        To = toValue;
        Duration = duration;
    }

    public MatrixAnimation(Matrix toValue, Duration duration, FillBehavior fillBehavior)
    {
        To = toValue;
        Duration = duration;
        FillBehavior = fillBehavior;
    }

    public MatrixAnimation(Matrix fromValue, Matrix toValue, Duration duration)
    {
        From = fromValue;
        To = toValue;
        Duration = duration;
    }

    public MatrixAnimation(Matrix fromValue, Matrix toValue, Duration duration, FillBehavior fillBehavior)
    {
        From = fromValue;
        To = toValue;
        Duration = duration;
        FillBehavior = fillBehavior;
    }

    protected override Freezable CreateInstanceCore() => new MatrixAnimation();

    protected override Matrix GetCurrentValueCore(Matrix defaultOriginValue, Matrix defaultDestinationValue, AnimationClock animationClock)
    {
        if (animationClock.CurrentProgress == null)
        {
            return Matrix.Identity;
        }

        var normalizedTime = animationClock.CurrentProgress.Value;
        if (EasingFunction != null)
        {
            normalizedTime = EasingFunction.Ease(normalizedTime);
        }

        var from = From ?? defaultOriginValue;
        var to = To ?? defaultDestinationValue;

        var newMatrix = new Matrix(
                (to.M11 - from.M11) * normalizedTime + from.M11,
                (to.M12 - from.M12) * normalizedTime + from.M12,
                (to.M21 - from.M21) * normalizedTime + from.M21,
                (to.M22 - from.M22) * normalizedTime + from.M22,
                (to.OffsetX - from.OffsetX) * normalizedTime + from.OffsetX,
                (to.OffsetY - from.OffsetY) * normalizedTime + from.OffsetY);

        return newMatrix;
    }
}

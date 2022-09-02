using System;
using Windows.UI.Composition;

namespace Stream.Wall.Composition
{
    public static class ImplicitAnimationFactory
    {
        public static CompositionAnimationGroup CreateListOffsetAnimationGroup(Compositor compositor)
        {
            Vector3KeyFrameAnimation offsetAnimation = compositor.CreateVector3KeyFrameAnimation();
            offsetAnimation.InsertExpressionKeyFrame(1f, "this.FinalValue");
            offsetAnimation.Duration = TimeSpan.FromMilliseconds(400);
            offsetAnimation.Target = "Offset";

            CompositionAnimationGroup animationGroup = compositor.CreateAnimationGroup();
            animationGroup.Add(offsetAnimation);

            return animationGroup;
        }

        public static ImplicitAnimationCollection CreateListOffsetAnimationCollection(Compositor compositor)
        {
            var collection = compositor.CreateImplicitAnimationCollection();
            collection["Offset"] = CreateListOffsetAnimationGroup(compositor); ;
            return collection;
        }

        public static ImplicitAnimationCollection CreateCommonOpacityAnimationCollection(Compositor compositor)
        {
            var collection = compositor.CreateImplicitAnimationCollection();
            collection["Opacity"] = CreateOpacityAnimation(compositor);
            return collection;
        }

        private static ScalarKeyFrameAnimation CreateOpacityAnimation(Compositor compositor)
        {
            var opacityAnimation = compositor.CreateScalarKeyFrameAnimation();
            opacityAnimation.Target = "Opacity";
            opacityAnimation.InsertExpressionKeyFrame(1.0f, "this.FinalValue");
            opacityAnimation.Duration = TimeSpan.FromMilliseconds(300);

            return opacityAnimation;
        }
    }
}

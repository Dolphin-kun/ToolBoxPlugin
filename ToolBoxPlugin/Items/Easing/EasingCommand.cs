using System.Windows;
using ToolBox.Commons;
using ToolBox.Items.Easing.Settings;
using YukkuriMovieMaker.Commons;

namespace ToolBox.Items.Easing
{
    internal static class EasingCommand
    {
        public static bool CanApply(IToolBoxContext ctx) =>
            ctx.SelectedItems?.Count > 0 && ctx.UndoRedoManager != null;

        public static void Apply(IToolBoxContext ctx)
        {
            if (ctx.SelectedItems is not { Count: > 0 } items || ctx.UndoRedoManager is not { } undoRedoManager)
                return;

            var settings = ctx.FindItemSettings<EasingSettings>() ?? new EasingSettings();
            if (ResolveType(settings) is not { } type)
                return;

            var changed = false;
            foreach (var item in items)
            {
                var animations = settings.Scope == EasingScope.Draw
                    ? AnimationHelper.GetDrawAnimations(item)
                    : AnimationHelper.GetAnimations(item);

                foreach (var animation in animations.Where(AnimationHelper.IsMoving))
                {
                    animation.AnimationType = type;

                    if (settings.IsBezier)
                        animation.Bezier.CopyFrom(settings.Bezier);

                    changed = true;
                }
            }

            if (changed)
                undoRedoManager.Record();
            else
                MessageBox.Show(Texts.EasingNoMovingParameters, Texts.EasingDisplayName, MessageBoxButton.OK);
        }

        private static AnimationType? ResolveType(EasingSettings settings)
        {
            if (settings.IsBezier)
                return settings.IsSmooth ? AnimationType.Interpolation_Bezier : AnimationType.Linear_Bezier;

            if (settings.Curve == EasingType.Linear)
                return settings.IsSmooth ? AnimationType.補間移動 : AnimationType.直線移動;

            if (settings.Curve == EasingType.Jump)
                return AnimationType.瞬間移動;

            var name = $"{settings.Curve}_{settings.Direction}{(settings.IsSmooth ? "_Int" : string.Empty)}";
            return Enum.TryParse<AnimationType>(name, out var type) ? type : null;
        }
    }
}

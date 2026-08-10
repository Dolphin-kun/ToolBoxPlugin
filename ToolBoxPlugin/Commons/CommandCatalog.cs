using ToolBox.Settings;

namespace ToolBox.Commons
{
    public record CommandCatalogEntry(string CommandName, string DisplayName, string GroupName)
    {
        public string FullDisplayName => $"{GroupName} / {DisplayName}";
    }

    public static class CommandCatalog
    {
        public static IReadOnlyList<CommandCatalogEntry> GetEntries()
        {
            var entries = new List<CommandCatalogEntry>();
            var seen = new HashSet<string>(StringComparer.Ordinal);

            foreach (var item in ItemCatalog.GetDraggableItems())
            {
                var groupName = ResolveGroupName(item);

                foreach (var definition in item.GetCommandDefinitions())
                {
                    if (definition.RequiresItemParameter)
                        continue;

                    if (!seen.Add(definition.Name))
                        continue;

                    entries.Add(new CommandCatalogEntry(definition.Name, definition.DisplayName, groupName));
                }
            }

            return [.. entries.OrderBy(e => e.GroupName, StringComparer.CurrentCulture)
                              .ThenBy(e => e.DisplayName, StringComparer.CurrentCulture)];
        }

        public static CommandCatalogEntry? Find(string commandName) =>
            GetEntries().FirstOrDefault(e => e.CommandName == commandName);

        private static string ResolveGroupName(DraggableItem item)
        {
            try
            {
                if (Activator.CreateInstance(item.SettingsType) is ToolItemSettings settings)
                    return settings.DisplayName;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to resolve group name for {item.SettingsType}: {ex}");
            }

            return item.SettingsType.Name;
        }
    }
}

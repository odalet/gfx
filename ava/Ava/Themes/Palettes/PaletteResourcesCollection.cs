using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Styling;

namespace Ava.Themes.Palettes;

internal sealed class PaletteResourcesCollection : ResourceProvider, IDictionary<ThemeVariant, PaletteResources>
{
    private readonly AvaloniaDictionary<ThemeVariant, PaletteResources> inner;

    public PaletteResourcesCollection()
    {
        inner = new(2);
        inner.ForEachItem(
            (k, v) =>
            {
                if (k != ThemeVariant.Dark && k != ThemeVariant.Light) throw new InvalidOperationException(
                    $"{nameof(AvaThemes)} Palettes only supports Light and Dark variants.");
                if (Owner is not null) ((IResourceProvider)v).AddOwner(Owner);
            },
            (_, v) =>
            {
                if (Owner is not null) ((IResourceProvider)v).RemoveOwner(Owner);
            },
            () => throw new NotSupportedException("Dictionary reset is not supported"));
    }

    public override bool HasResources => inner.Count > 0;
    public int Count => inner.Count;
    public bool IsReadOnly => inner.IsReadOnly;
    public ICollection<ThemeVariant> Keys => inner.Keys;
    public ICollection<PaletteResources> Values => inner.Values;
    public PaletteResources this[ThemeVariant key]
    {
        get =>  inner[key];
        set =>  inner[key] = value;
    }

    public override bool TryGetResource(object key, ThemeVariant? theme, out object? value)
    {
        if (theme == null || theme == ThemeVariant.Default)
            theme = ThemeVariant.Light;

        if (inner.TryGetValue(theme, out var themePaletteResources) &&
            themePaletteResources.TryGetResource(key, theme, out value))
            return true;

        value = null;
        return false;
    }

    public IEnumerator<KeyValuePair<ThemeVariant, PaletteResources>> GetEnumerator() => inner.GetEnumerator();
    public void Add(ThemeVariant key, PaletteResources value) => inner.Add(key, value);
    public bool Remove(ThemeVariant key) => inner.Remove(key);
    public void Clear() => inner.Clear();
    public void CopyTo(KeyValuePair<ThemeVariant, PaletteResources>[] array, int arrayIndex) => inner.CopyTo(array, arrayIndex);
    public bool ContainsKey(ThemeVariant key) => inner.ContainsKey(key);
    public bool TryGetValue(ThemeVariant key, [MaybeNullWhen(false)] out PaletteResources value) => inner.TryGetValue(key, out value);
    
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    void ICollection<KeyValuePair<ThemeVariant, PaletteResources>>.Add(KeyValuePair<ThemeVariant, PaletteResources> item) => ((ICollection<KeyValuePair<ThemeVariant, PaletteResources>>)inner).Add(item);
    bool ICollection<KeyValuePair<ThemeVariant, PaletteResources>>.Contains(KeyValuePair<ThemeVariant, PaletteResources> item) => ((ICollection<KeyValuePair<ThemeVariant, PaletteResources>>)inner).Contains(item);
    bool ICollection<KeyValuePair<ThemeVariant, PaletteResources>>.Remove(KeyValuePair<ThemeVariant, PaletteResources> item) =>  ((ICollection<KeyValuePair<ThemeVariant, PaletteResources>>)inner).Remove(item);

    protected override void OnAddOwner(IResourceHost owner)
    {
        base.OnAddOwner(owner);
        foreach (var palette in inner.Values)
            ((IResourceProvider)palette).AddOwner(owner);
    }

    protected override void OnRemoveOwner(IResourceHost owner)
    {
        base.OnRemoveOwner(owner);
        foreach (var palette in inner.Values)
            ((IResourceProvider)palette).RemoveOwner(owner);
    }
}
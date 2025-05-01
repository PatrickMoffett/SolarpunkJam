using System.Collections.Generic;
using UnityEngine;

public class CombatTagContainer : MonoBehaviour
{
    Dictionary<CombatTag,int> _combatTagsSet = new Dictionary<CombatTag, int>();

    public void AddTag(CombatTag tag)
    {
        if (tag == null) return;

        if (_combatTagsSet.ContainsKey(tag))
        {
            _combatTagsSet[tag]++;
        }
        else
        {
            _combatTagsSet[tag] = 1;
        }
    }
    public void AddTags(List<CombatTag> tags)
    {
        if (tags == null) { return; }
        foreach (var tag in tags)
        {
            AddTag(tag);
        }
    }
    public bool HasTag(CombatTag tag)
    {
        if (tag == null) { return false; }
        return _combatTagsSet.ContainsKey(tag);
    }
    public bool HasAnyTag(List<CombatTag> tags)
    {
        if (tags == null) { return false; }
        foreach (var tag in tags)
        {
            if (HasTag(tag)) { return true; }
        }
        return false;
    }
    public void RemoveTag(CombatTag tag)
    {
        if (tag == null) { return; }
        if (!_combatTagsSet.ContainsKey(tag)) { return; }
        _combatTagsSet[tag]--;
        if (_combatTagsSet[tag] <= 0)
        {
            _combatTagsSet.Remove(tag);
        }
    }
    public void RemoveTags(List<CombatTag> tags)
    {
        if (tags == null) { return; }
        foreach (var tag in tags)
        {
            RemoveTag(tag);
        }
    }
}
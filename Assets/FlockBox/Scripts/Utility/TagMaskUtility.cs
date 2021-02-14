using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This utility was written to port the tag masking system into a DOTS-friendly data structure.
/// </summary>
public static class TagMaskUtility
{
    private static List<string> _tagRegistry;

    public static byte TagToInt(string tag)
    {
        if (_tagRegistry==null) _tagRegistry = new List<string>();
        if (!_tagRegistry.Contains(tag))
        {
            _tagRegistry.Add(tag);
        }
        return (byte)_tagRegistry.IndexOf(tag);
    }

    public static int GetTagMask(params string[] args)
    {
        int mask = 0;
        foreach(string arg in args)
        {
            mask = mask | 1 << TagToInt(arg);
        }

        return mask;
    }

    public static void AddTagToMask(string tag, ref int mask)
    {
        mask = mask | 1 << TagToInt(tag);
    }

    public static void AddTagToMask(byte tag, ref int mask)
    {
        mask = mask | 1 << tag;
    }

    public static bool TagInMask(string tag, int mask)
    {
        if (mask == 0) return true;
        return TagInMask(TagToInt(tag), mask);
    }

    public static bool TagInMask(byte tag, int mask)
    {
        if (mask == 0) return true;
        return (1 << tag & mask) != 0;
    }
}

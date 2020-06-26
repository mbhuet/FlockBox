using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    public static Int32 GetTagMask(params string[] args)
    {
        Int32 mask = 0;
        foreach(string arg in args)
        {
            mask = mask | 1 << TagToInt(arg);
        }

        return mask;
    }

    public static bool TagInMask(string tag, Int32 mask)
    {
        return TagInMask(TagToInt(tag), mask);
    }

    public static bool TagInMask(byte tag, Int32 mask)
    {
        return (1 << tag & mask) != 0;

    }
}

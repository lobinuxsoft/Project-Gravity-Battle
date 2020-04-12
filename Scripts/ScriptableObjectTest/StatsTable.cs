using Godot;
using System;
using Godot.Collections;

public class StatsTable : Resource
{
    [Export] private Dictionary<String, CalssStats> classList = new Dictionary<String, CalssStats>();
}
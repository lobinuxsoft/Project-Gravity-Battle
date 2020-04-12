using System;
using Godot;

public class CalssStats : Resource
{
    [Export] public int Health { get; set; }

    [Export] public Resource SubResource { get; set; }

    [Export] public String[] Strings { get; set; }
}
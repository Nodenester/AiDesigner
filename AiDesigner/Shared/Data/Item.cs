using System;
using System.Collections.Generic;
using NodeBaseApi.Version2;

public class Item
{
    public string Name { get; set; }
    public List<Item> SubItems { get; set; }
    public bool IsExpanded { get; set; }
    public Block block { get; set; }   
    public bool IsTooltipVisible { get; set; }
}



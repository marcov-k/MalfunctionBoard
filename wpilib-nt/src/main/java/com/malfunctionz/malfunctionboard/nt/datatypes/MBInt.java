package com.malfunctionz.malfunctionboard.nt.datatypes;

public record MBInt(String type, int value) implements MBData
{
    public MBInt(int value)
    {
        this("Int", value);
    }
}

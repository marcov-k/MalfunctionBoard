package com.malfunctionz.malfunctionboard.nt.datatypes;

public record MBBool(String type, boolean value)
{
    public MBBool(boolean value)
    {
        this("Bool", value);
    }
}

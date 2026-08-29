package com.malfunctionz.malfunctionboard.nt;

import edu.wpi.first.networktables.NetworkTableEntry;
import edu.wpi.first.networktables.NetworkTable;
import edu.wpi.first.networktables.NetworkTableInstance;

public class MalfunctionBoard
{
    NetworkTable networkTable;

    public MalfunctionBoard(String networkTableName)
    {
        networkTable = NetworkTableInstance.getDefault().getTable(networkTableName);
    }

    public void SetDouble(String entryName, double value)
    {
        getEntry(entryName).setDouble(value);
    }

    public void SetInt(String entryName, long value)
    {
        getEntry(entryName).setInteger(value);
    }

    public void SetString(String entryName, String value)
    {
        getEntry(entryName).setString(value);
    }

    NetworkTableEntry getEntry(String entryName)
    {
        return networkTable.getEntry(entryName);
    }
}

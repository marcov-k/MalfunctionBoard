package com.malfunctionz.malfunctionboard.nt;

import com.google.gson.FieldNamingPolicy;
import com.google.gson.Gson;
import com.google.gson.GsonBuilder;
import com.malfunctionz.malfunctionboard.nt.datatypes.*;
import edu.wpi.first.networktables.NetworkTable;
import edu.wpi.first.networktables.NetworkTableInstance;
import edu.wpi.first.networktables.StringPublisher;
import java.util.HashMap;

public class MalfunctionBoard
{
    static final Gson gson = new GsonBuilder()
        .setFieldNamingPolicy(FieldNamingPolicy.UPPER_CAMEL_CASE)
        .create();

    final NetworkTable networkTable;
    final HashMap<String, StringPublisher> publisherCache = new HashMap<>();

    public MalfunctionBoard(String networkTableName)
    {
        networkTable = NetworkTableInstance.getDefault().getTable(networkTableName);
    }

    public void writeInt(String entryName, MBInt value)
    {
        writeDataToEntry(getEntry(entryName), value);
    }

    public void writeDouble(String entryName, MBDouble value)
    {
        writeDataToEntry(getEntry(entryName), value);
    }

    public void writeString(String entryName, MBString data)
    {
        writeDataToEntry(getEntry(entryName), data);
    }

    public void writeBool(String entryName, MBBool value)
    {
        writeDataToEntry(getEntry(entryName), value);
    }

    StringPublisher getEntry(String entryName)
    {
        return publisherCache.computeIfAbsent(entryName, name -> networkTable.getStringTopic(name).publish());
    }

    static <T> void writeDataToEntry(StringPublisher entry, T data)
    {
        String json = gson.toJson(data);
        entry.set(json);
    }

    public void close()
    {
        for (StringPublisher publisher : publisherCache.values())
        {
            publisher.close();
        }
        publisherCache.clear();
    }
}

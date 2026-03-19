using UnityEngine;

using System.Collections;
using System;
using System.Runtime.InteropServices;
using System.IO;
using System.Text;

public class CAccount 
{
    public struct ACCOUNT_DATA
    {
        public DateTime date;
        public int coin;
    };

    const int m_nTotalNum = 31;
    public static int m_nTotalCoin = 0;
    public static ACCOUNT_DATA[] m_Account;
    static string m_Filename;
        
    public static void Init() 
    {
        m_Account = new ACCOUNT_DATA[m_nTotalNum];

        m_Filename = Application.dataPath + "/account.dat";

        if (!File.Exists(m_Filename)) return;
        StreamReader file = new StreamReader(m_Filename, Encoding.Default, true);
        string s;
        s = file.ReadLine();
        m_nTotalCoin = Convert.ToInt32(s);
        for (int i = 0; i < m_nTotalNum; i++)
        {
            s = file.ReadLine();
            m_Account[i].date = Convert.ToDateTime(s);
            s = file.ReadLine();
            m_Account[i].coin = Convert.ToInt32(s);
        }
        file.Close();
    }

    public static void AddCoin(int coin) 
    {
        DateTime now = DateTime.Now;
        if (m_Account[0].date.ToShortDateString() != now.ToShortDateString()) NextDay();
        m_Account[0].date = now;
        m_Account[0].coin += coin;
        m_nTotalCoin += coin;
        Save();
	}

    static void NextDay()
    {
        for (int i = m_nTotalNum - 1; i > 0 ; i--)
        {
            m_Account[i].date = m_Account[i - 1].date;
            m_Account[i].coin = m_Account[i - 1].coin;
        }
        m_Account[0].coin = 0;
    }

    static void Save()
    {
        StreamWriter file = new StreamWriter(m_Filename, false, Encoding.Default);
        file.WriteLine(m_nTotalCoin.ToString());
        for (int i = 0; i < m_nTotalNum; i++)
        {
            string s = m_Account[i].date.ToShortDateString();
            file.WriteLine(s);
            file.WriteLine(m_Account[i].coin.ToString());
        }
        file.Close();
    }
}

using PartySchoolApi.Models.Entities;

namespace PartySchoolApi.Services.Common;

/// <summary>
/// 组织架构层级工具：提供「组织 + 全部下级组织」的递归汇总能力。
/// 所有需要按组织口径统计的地方（支部考核报告、大屏排名、AI数据问答、工作台等）统一使用本工具，
/// 保证「党总支 = 其下所有支部的党员/任务/成绩」的汇总口径一致。
/// </summary>
public static class OrgHierarchyHelper
{
    /// <summary>收集某组织的全部子孙组织Id（不含自身）</summary>
    public static List<int> CollectDescendantOrgIds(int parentId, IReadOnlyList<Organization> all)
    {
        var ids = new List<int>();
        foreach (var child in all.Where(o => o.ParentId == parentId))
        {
            ids.Add(child.Id);
            ids.AddRange(CollectDescendantOrgIds(child.Id, all));
        }
        return ids;
    }

    /// <summary>收集某组织及全部子孙组织的Id集合（含自身）</summary>
    public static List<int> CollectOrgAndDescendantIds(int rootId, IReadOnlyList<Organization> all)
    {
        var ids = new List<int> { rootId };
        ids.AddRange(CollectDescendantOrgIds(rootId, all));
        return ids;
    }

    /// <summary>为每个组织预计算「自身+全部下级」的组织Id集合</summary>
    public static Dictionary<int, List<int>> BuildOrgScopeMap(IReadOnlyList<Organization> all)
    {
        var map = new Dictionary<int, List<int>>();
        foreach (var org in all)
        {
            map[org.Id] = CollectOrgAndDescendantIds(org.Id, all);
        }
        return map;
    }
}

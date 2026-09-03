$base = "http://localhost:5091/api/v1"
$adminLogin = Invoke-RestMethod -Uri "$base/auth/login" -Method POST -ContentType "application/json" -Body '{"phone":"13800000000","password":"123456"}'
$memberLogin = Invoke-RestMethod -Uri "$base/auth/login" -Method POST -ContentType "application/json" -Body '{"phone":"13800000002","password":"123456"}'
$at = $adminLogin.data.accessToken
$mt = $memberLogin.data.accessToken
$ah = @{ Authorization = "Bearer $at" }
$mh = @{ Authorization = "Bearer $mt" }

function Test-Endpoint($name, $method, $url, $body, $headers) {
    try {
        $params = @{ Uri = $url; Method = $method; Headers = $headers; ContentType = "application/json"; ErrorAction = "Stop" }
        if ($body) { $params.Body = $body }
        $r = Invoke-RestMethod @params
        $code = $r.code
        $dataStr = if ($r.data) { ($r.data | ConvertTo-Json -Depth 3 -Compress).Substring(0, [Math]::Min(300, ($r.data | ConvertTo-Json -Depth 3 -Compress).Length)) } else { "null" }
        Write-Host "[OK] $name => code=$code | $dataStr"
    } catch {
        $status = $_.Exception.Response.StatusCode.value__
        $msg = $_.Exception.Message
        try { $sr = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream()); $body = $sr.ReadToEnd(); $msg = $body.Substring(0, [Math]::Min(200, $body.Length)) } catch {}
        Write-Host "[FAIL] $name => HTTP $status | $msg"
    }
}

Write-Host "========== AI P0 FEATURES =========="
Test-Endpoint "1.RAG问答" "POST" "$base/ai-knowledge/query" '{"question":"什么是三会一课","topK":3}' $mh
Test-Endpoint "2.错题聚类" "POST" "$base/kmeans/cluster" '{"partyMemberId":3,"clusterCount":3}' $mh
Test-Endpoint "3.个性化推荐" "GET" "$base/mobile/recommendations?limit=5" $null $mh
Test-Endpoint "4.学习路线图" "POST" "$base/ai/learning-roadmap" '{"periodDays":30,"target":"系统学习党的二十大精神"}' $mh
Test-Endpoint "5.个人AI报告" "POST" "$base/mobile/report/ai-assessment" '{}' $mh
Test-Endpoint "6.NL2SQL" "POST" "$base/nl2sql/query" '{"naturalLanguage":"第一支部的党员人数"}' $ah
Test-Endpoint "7.AI素材生成-文章" "POST" "$base/ai-content/generate" '{"contentType":"article","topic":"党的二十大精神学习心得","maxWords":500}' $ah
Test-Endpoint "8.AI素材生成-宣讲稿" "POST" "$base/ai-content/generate" '{"contentType":"speech","topic":"七一建党节","audience":"党员","durationMinutes":10}' $ah
Test-Endpoint "9.AI素材生成-知识卡片" "POST" "$base/ai-content/generate" '{"contentType":"quizcard","topic":"党章基础知识"}' $ah
Test-Endpoint "10.AI评选标兵" "POST" "$base/ai/star-members" '{"topN":5,"includeReason":false}' $ah
Test-Endpoint "11.三会一课简报" "POST" "$base/meeting-activities/ai-brief" '{"startDate":"2026-01-01","endDate":"2026-12-31"}' $ah
Test-Endpoint "12.支部季度报告" "POST" "$base/ai/organization-report" '{"organizationId":4,"quarter":"2026Q3"}' $ah

Write-Host "========== BASIC P0 FEATURES =========="
Test-Endpoint "13.驾驶舱大屏" "GET" "$base/statistics/dashboard-largescreen" $null $ah
Test-Endpoint "14.通知列表" "GET" "$base/notifications/all?page=1&size=5" $null $mh
Test-Endpoint "15.考试列表" "GET" "$base/mobile/exams" $null $mh
Test-Endpoint "16.学习内容" "GET" "$base/mobile/contents" $null $mh
Test-Endpoint "17.任务列表" "GET" "$base/mobile/tasks" $null $mh
Test-Endpoint "18.个人概览" "GET" "$base/mobile/report/overview" $null $mh
Test-Endpoint "19.教育基地列表" "GET" "$base/education-sites?page=1&size=5" $null $mh
Test-Endpoint "20.防挂机挑战" "GET" "$base/anti-cheat/challenge?random=true" $null $mh
Test-Endpoint "21.积分明细" "GET" "$base/points/my?page=1&size=5" $null $mh
Test-Endpoint "22.发展台账" "GET" "$base/party-development?page=1&size=5" $null $ah
Test-Endpoint "23.组织生活列表" "GET" "$base/meeting-activities?page=1&size=5" $null $ah
Test-Endpoint "24.整改列表" "GET" "$base/organizations/4/rectifications?page=1&size=5" $null $ah
Test-Endpoint "25.结对我的" "GET" "$base/pair-help/my" $null $mh
Test-Endpoint "26.对战待应战" "GET" "$base/battles/pending" $null $mh
Test-Endpoint "27.打卡历史" "GET" "$base/check-in/my?page=1&size=5" $null $mh

Write-Host "========== DONE =========="

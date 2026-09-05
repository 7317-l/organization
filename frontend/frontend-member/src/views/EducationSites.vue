<template>
  <div class="education-sites-page">
    <div class="page-header">
      <h2>红色教育基地</h2>
    </div>

    <!-- 基地列表 -->
    <div class="site-list" v-loading="loading">
      <div v-for="site in sites" :key="site.id" class="site-card" @click="viewDetail(site)">
        <div class="site-image" v-if="site.coverImage || site.imageUrl">
          <img :src="site.coverImage || site.imageUrl" :alt="site.name" />
        </div>
        <div class="site-image placeholder" v-else>
          <el-icon :size="40"><Picture /></el-icon>
        </div>
        <div class="site-info">
          <div class="site-name">{{ site.name }}</div>
          <div class="site-address">📍 {{ site.address || site.location || '暂无地址' }}</div>
          <div class="site-desc">{{ site.description || site.intro || '暂无简介' }}</div>
          <div class="site-stats">
            <span>打卡 {{ site.checkinCount || 0 }} 次</span>
          </div>
        </div>
      </div>
      <el-empty v-if="sites.length === 0 && !loading" description="暂无红色教育基地" />
    </div>

    <!-- 基地详情弹窗 -->
    <el-dialog v-model="detailVisible" title="基地详情" width="500px">
      <div v-if="currentSite">
        <h3>{{ currentSite.name }}</h3>
        <p><strong>地址：</strong>{{ currentSite.address || currentSite.location || '暂无' }}</p>
        <p><strong>简介：</strong>{{ currentSite.description || currentSite.intro || '暂无' }}</p>
        <el-button type="primary" @click="openCheckinDialog">立即打卡</el-button>
      </div>
    </el-dialog>

    <!-- 打卡弹窗 -->
    <el-dialog v-model="checkinVisible" title="红色基地打卡" width="500px">
      <div class="checkin-form">
        <div class="photo-upload">
          <div class="photo-preview" v-if="photoPreview" @click="triggerPhotoInput">
            <img :src="photoPreview" alt="打卡照片" />
          </div>
          <div class="photo-placeholder" v-else @click="triggerPhotoInput">
            <el-icon :size="32"><Camera /></el-icon>
            <p>点击拍照/上传照片</p>
          </div>
          <input ref="photoInput" type="file" accept="image/*" style="display:none" @change="handlePhotoChange" />
        </div>
        <el-input
          v-model="checkinContent"
          type="textarea"
          :rows="4"
          placeholder="请输入打卡感悟（100-500字）..."
          maxlength="500"
          show-word-limit
        />
      </div>
      <template #footer>
        <el-button @click="checkinVisible = false">取消</el-button>
        <el-button type="primary" :loading="checkinSubmitting" @click="submitCheckin">提交打卡</el-button>
      </template>
    </el-dialog>

    <!-- 我的打卡历史 -->
    <div class="my-checkins" v-if="myCheckins.length > 0">
      <h3>我的打卡记录</h3>
      <div class="timeline">
        <div v-for="record in myCheckins" :key="record.id" class="timeline-item">
          <div class="timeline-dot"></div>
          <div class="timeline-content">
            <div class="timeline-title">{{ record.locationName }}</div>
            <div class="timeline-time">{{ formatDate(record.checkInTime) }}</div>
            <div class="timeline-points">+{{ record.pointsEarned }}积分</div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { ElMessage } from 'element-plus'
import { Picture, Camera } from '@element-plus/icons-vue'
import request from '@/api/request'

const loading = ref(false)
const sites = ref([])
const detailVisible = ref(false)
const checkinVisible = ref(false)
const checkinSubmitting = ref(false)
const currentSite = ref(null)
const checkinContent = ref('')
const photoPreview = ref('')
const photoInput = ref(null)
const myCheckins = ref([])

function formatDate(date) {
  if (!date) return ''
  return new Date(date).toLocaleString('zh-CN')
}

async function loadSites() {
  loading.value = true
  try {
    const res = await request.get('/education-sites', { params: { page: 1, size: 50 } })
    sites.value = res.items || res.data || []
  } catch (e) {
    ElMessage.error('加载基地列表失败')
  } finally {
    loading.value = false
  }
}

async function loadMyCheckins() {
  try {
    const res = await request.get('/education-sites/my-checkins', { params: { page: 1, size: 20 } })
    myCheckins.value = res.items || res.data?.items || []
  } catch (e) { /* */ }
}

function viewDetail(site) {
  currentSite.value = site
  detailVisible.value = true
}

function openCheckinDialog() {
  detailVisible.value = false
  checkinContent.value = ''
  photoPreview.value = ''
  checkinVisible.value = true
}

function triggerPhotoInput() {
  photoInput.value?.click()
}

function handlePhotoChange(e) {
  const file = e.target.files[0]
  if (file) {
    const reader = new FileReader()
    reader.onload = (ev) => {
      photoPreview.value = ev.target.result
    }
    reader.readAsDataURL(file)
  }
}

async function submitCheckin() {
  if (!checkinContent.value.trim()) return ElMessage.warning('请输入打卡感悟')
  if (checkinContent.value.trim().length < 10) return ElMessage.warning('感悟至少10个字')
  checkinSubmitting.value = true
  try {
    await request.post(`/education-sites/${currentSite.value.id}/checkin`, {
      content: checkinContent.value,
      photoUrl: photoPreview.value
    })
    ElMessage.success('打卡成功，获得5积分')
    checkinVisible.value = false
    loadMyCheckins()
  } catch (e) {
    ElMessage.error('打卡失败')
  } finally {
    checkinSubmitting.value = false
  }
}

onMounted(() => {
  loadSites()
  loadMyCheckins()
})
</script>

<style scoped>
.education-sites-page { padding: 16px; }
.page-header { margin-bottom: 16px; }
.page-header h2 { margin: 0; font-size: 20px; }
.site-list { display: flex; flex-direction: column; gap: 12px; }
.site-card { background: #fff; border-radius: 8px; overflow: hidden; box-shadow: 0 2px 8px rgba(0,0,0,0.08); cursor: pointer; display: flex; }
.site-image { width: 120px; height: 120px; flex-shrink: 0; background: #f5f5f5; display: flex; align-items: center; justify-content: center; }
.site-image img { width: 100%; height: 100%; object-fit: cover; }
.site-image.placeholder { color: #ccc; }
.site-info { padding: 12px; flex: 1; }
.site-name { font-size: 16px; font-weight: bold; margin-bottom: 4px; }
.site-address { color: #666; font-size: 13px; margin-bottom: 4px; }
.site-desc { color: #999; font-size: 12px; margin-bottom: 4px; overflow: hidden; text-overflow: ellipsis; display: -webkit-box; -webkit-line-clamp: 2; -webkit-box-orient: vertical; }
.site-stats { color: #C8161D; font-size: 12px; }
.checkin-form { display: flex; flex-direction: column; gap: 16px; }
.photo-upload { display: flex; justify-content: center; }
.photo-preview, .photo-placeholder { width: 200px; height: 150px; border: 2px dashed #ddd; border-radius: 8px; display: flex; align-items: center; justify-content: center; cursor: pointer; flex-direction: column; color: #999; }
.photo-preview img { width: 100%; height: 100%; object-fit: cover; border-radius: 6px; }
.my-checkins { margin-top: 24px; }
.my-checkins h3 { margin-bottom: 16px; }
.timeline { position: relative; padding-left: 20px; }
.timeline-item { position: relative; padding-bottom: 16px; }
.timeline-dot { position: absolute; left: -20px; top: 4px; width: 12px; height: 12px; border-radius: 50%; background: #C8161D; }
.timeline-content { background: #fff; padding: 12px; border-radius: 8px; box-shadow: 0 2px 4px rgba(0,0,0,0.05); }
.timeline-title { font-weight: bold; margin-bottom: 4px; }
.timeline-time { color: #999; font-size: 12px; }
.timeline-points { color: #C8161D; font-size: 12px; margin-top: 4px; }
</style>

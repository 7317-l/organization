<template>
  <el-button
    size="small"
    :type="listening ? 'danger' : 'primary'"
    :icon="Microphone"
    :title="listening ? '点击停止' : '语音输入'"
    @click="toggle"
  >{{ listening ? '停止' : '语音' }}</el-button>
</template>

<script setup>
import { ref, onBeforeUnmount } from 'vue'
import { ElMessage } from 'element-plus'
import { Microphone } from '@element-plus/icons-vue'
import { speechSupported, createRecognizer } from './common'

const props = defineProps({
  modelValue: { type: String, default: '' }
})
const emit = defineEmits(['update:modelValue'])

const listening = ref(false)
let rec = null

function toggle() {
  if (listening.value) {
    stop()
    return
  }
  if (!speechSupported()) {
    ElMessage.warning('当前浏览器不支持语音识别，请使用 Chrome/Edge')
    return
  }
  try {
    rec = createRecognizer(
      (text) => {
        const base = props.modelValue || ''
        emit('update:modelValue', base ? base + ' ' + text : text)
      },
      () => { listening.value = false },
      (msg) => { ElMessage.warning(msg); listening.value = false }
    )
    if (rec) {
      rec.start()
      listening.value = true
      ElMessage.info('开始聆听，请说话…')
    }
  } catch (e) {
    ElMessage.warning('语音启动失败：' + (e?.message || e))
    listening.value = false
  }
}

function stop() {
  try { rec && rec.stop() } catch (e) { /* ignore */ }
  listening.value = false
}

onBeforeUnmount(() => stop())
</script>

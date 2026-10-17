import { createApp } from 'vue'
import App from './App.vue'
import router from './router'
import './styles/admin.css'
import {
  ArrowDown,
  Bell,
  DataAnalysis,
  Document,
  Files,
  FolderOpened,
  Headset,
  Hide,
  InfoFilled,
  Lock,
  Monitor,
  Notebook,
  Plus,
  Reading,
  Refresh,
  Search,
  SwitchButton,
  Upload,
  UploadFilled,
  User,
  UserFilled,
  View,
} from '@element-plus/icons-vue'

const app = createApp(App)

const icons = {
  ArrowDown,
  Bell,
  DataAnalysis,
  Document,
  Files,
  FolderOpened,
  Headset,
  Hide,
  InfoFilled,
  Lock,
  Monitor,
  Notebook,
  Plus,
  Reading,
  Refresh,
  Search,
  SwitchButton,
  Upload,
  UploadFilled,
  User,
  UserFilled,
  View,
}
for (const [name, component] of Object.entries(icons)) {
  app.component(name, component)
}

app.use(router)
app.mount('#app')

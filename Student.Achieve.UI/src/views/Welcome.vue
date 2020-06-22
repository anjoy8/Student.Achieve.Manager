<template>
    <div style="margin-top: 30px;">
          <div style="margin: 20px;"> 欢迎来到学生成绩管理系统!</div>

<el-card class="welcome-card">
    <el-aside>操作指南：</el-aside>
 <div v-for="o in 4" :key="o" class="text item">
    {{'列表内容 ' + o }}
  </div>
    <br>
</el-card>

    </div>
</template>

<script>
    export default {
        name: "Welcome",
        mounted() {
            var curTime = new Date()
            if(window.localStorage.TokenExpire){
                var expiretime = new Date(Date.parse(window.localStorage.TokenExpire))
                if(curTime>=expiretime){
                    this.$router.push('/login');
                }
            }else {
                this.$router.push('/login');
            }

        },
    }
</script>

<style scoped>

</style>

<template>
  <div>
    <div class="note-alert">注意：提交的资料，从上自下，先提交教师资料，否则无法创建相应的年级和科目</div>
    <el-card class="box-card">
      <!-- action必选参数, 上传的地址 -->
      <div slot="header" class="clearfix">
        <span>教师资料</span>
        <el-button style="float: right; padding: 3px 0" type="text"></el-button>
      </div>
      <el-upload
        :disabled="videoFlagDist"
        class="avatar-uploader el-upload--text"
        :action="uploadUrlt"
        accept=".xlsx"
        :show-file-list="false"
        :on-success="handleVideoSuccess"
        :before-upload="beforeUploadVideo"
        :on-progress="uploadVideoProcessT"
      >
        <el-button :disabled="videoFlagDist" size="small" type="primary">点击上传《教师资料》Excel数据</el-button>

        <el-progress
          v-if="videoFlagt == true"
          type="circle"
          :percentage="percentage"
          style="margin-top:30px;"
        ></el-progress>
      </el-upload>
      <P class="text">只能上传Excel文件，且不超过10M</P>
    </el-card>

    <el-card class="box-card">
      <!-- action必选参数, 上传的地址 -->
      <div slot="header" class="clearfix">
        <span>学生资料</span>
        <el-button style="float: right; padding: 3px 0" type="text"></el-button>
      </div>
      <el-upload
        :disabled="videoFlagDiss"
        class="avatar-uploader el-upload--text"
        :action="uploadUrls"
        accept=".xlsx"
        :show-file-list="false"
        :on-success="handleVideoSuccess"
        :before-upload="beforeUploadVideo"
        :on-progress="uploadVideoProcessS"
      >
        <el-button :disabled="videoFlagDiss" size="small" type="primary">点击上传《学生资料》Excel数据</el-button>

        <el-progress
          v-if="videoFlags == true"
          type="circle"
          :percentage="percentage"
          style="margin-top:30px;"
        ></el-progress>
      </el-upload>
      <P class="text">只能上传Excel文件，且不超过10M</P>
    </el-card>

    <el-card class="box-card">
      <!-- action必选参数, 上传的地址 -->
      <div slot="header" class="clearfix">
        <span>六次考试成绩资料</span>
        <el-button style="float: right; padding: 3px 0" type="text"></el-button>
      </div>
      <el-upload
        class="avatar-uploader el-upload--text"
        :action="uploadUrles"
        :disabled="videoFlagDises"
        accept=".xlsx"
        :show-file-list="false"
        :on-success="handleVideoSuccess"
        :before-upload="beforeUploadVideo"
        :on-progress="uploadVideoProcessES"
      >
        <el-button :disabled="videoFlagDises" size="small" type="primary">点击上传《六次考试成绩资料》Excel数据</el-button>

        <el-progress
          v-if="videoFlages == true"
          type="circle"
          :percentage="percentage"
          style="margin-top:30px;"
        ></el-progress>
      </el-upload>
      <P class="text">只能上传Excel文件，且不超过10M</P>
    </el-card>

    

    <el-card class="box-card">
      <!-- action必选参数, 上传的地址 -->
      <div slot="header" class="clearfix">
        <span>学生基础名次资料</span>
        <el-button style="float: right; padding: 3px 0" type="text"></el-button>
      </div>
      <el-upload
        class="avatar-uploader el-upload--text"
        :action="uploadUrlbs"
        :disabled="videoFlagDisbs"
        accept=".xlsx"
        :show-file-list="false"
        :on-success="handleVideoSuccess"
        :before-upload="beforeUploadVideo"
        :on-progress="uploadVideoProcessBS"
      >
        <el-button :disabled="videoFlagDisbs" size="small" type="primary">点击上传《学生基础名次资料》Excel数据</el-button>

        <el-progress
          v-if="videoFlages == true"
          type="circle"
          :percentage="percentage"
          style="margin-top:30px;"
        ></el-progress>
      </el-upload>
      <P class="text">只能上传Excel文件，且不超过10M</P>
    </el-card>

    
    <el-card class="box-card">
      <!-- action必选参数, 上传的地址 -->
      <div slot="header" class="clearfix">
        <span>单科客观题选题情况资料 </span>
        <el-button style="float: right; padding: 3px 0" type="text"></el-button>
      </div>
      <el-upload
        class="avatar-uploader el-upload--text"
        :action="uploadUrled"
        :disabled="videoFlagDised"
        accept=".xlsx"
        :show-file-list="false"
        :on-success="handleVideoSuccess"
        :before-upload="beforeUploadVideo"
        :on-progress="uploadVideoProcessED"
      >
        <el-button :disabled="videoFlagDised" size="small" type="primary">点击上传《单科客观题选题资料》Excel数据</el-button>

        <el-progress
          v-if="videoFlaged == true"
          type="circle"
          :percentage="percentage"
          style="margin-top:30px;"
        ></el-progress>
      </el-upload>
      <P class="text">只能上传Excel文件，且不超过10M</P>
    </el-card>
    <el-card class="box-card">
      <!-- action必选参数, 上传的地址 -->
      <div slot="header" class="clearfix">
        <span>单科成绩资料</span>
        <el-button style="float: right; padding: 3px 0" type="text"></el-button>
      </div>
      <el-upload
        class="avatar-uploader el-upload--text"
        :action="uploadUrleds"
        :disabled="videoFlagDiseds"
        accept=".xlsx"
        :show-file-list="false"
        :on-success="handleVideoSuccess"
        :before-upload="beforeUploadVideo"
        :on-progress="uploadVideoProcessEDS"
      >
        <el-button :disabled="videoFlagDiseds" size="small" type="primary">点击上传《单科成绩资料》Excel数据</el-button>

        <el-progress
          v-if="videoFlageds == true"
          type="circle"
          :percentage="percentage"
          style="margin-top:30px;"
        ></el-progress>
      </el-upload>
      <P class="text">只能上传Excel文件，且不超过10M</P>
    </el-card>

    
    <el-card class="box-card">
      <!-- action必选参数, 上传的地址 -->
      <div slot="header" class="clearfix">
        <span>单科考题资料</span>
        <el-button style="float: right; padding: 3px 0" type="text"></el-button>
      </div>
      <el-upload
        class="avatar-uploader el-upload--text"
        :action="uploadUrledsa"
        :disabled="videoFlagDisedsa"
        accept=".xlsx"
        :show-file-list="false"
        :on-success="handleVideoSuccess"
        :before-upload="beforeUploadVideo"
        :on-progress="uploadVideoProcessEDSA"
      >
        <el-button :disabled="videoFlagDisedsa" size="small" type="primary">点击上传《单科考题资料》Excel数据</el-button>

        <el-progress
          v-if="videoFlagedsa == true"
          type="circle"
          :percentage="percentage"
          style="margin-top:30px;"
        ></el-progress>
      </el-upload>
      <P class="text">只能上传Excel文件，且不超过10M</P>
    </el-card>

  </div>
</template>>


<script>
export default {
  data() {
    return {
      percentage: 1,
      uploadUrlt: "/api/ExcelImport/ImportTeacherDataFile?datatype=t",
      videoFlagt: false,
      videoFlagDist: false,
      uploadUrls: "/api/ExcelImport/ImportTeacherDataFile?datatype=s",
      videoFlags: false,
      videoFlagDiss: false,
      uploadUrles: "/api/ExcelImport/ImportTeacherDataFile?datatype=es",
      videoFlages: false,
      videoFlagDises: false,
      uploadUrlbs: "/api/ExcelImport/ImportTeacherDataFile?datatype=bs",
      videoFlagbs: false,
      videoFlagDisbs: false,
      uploadUrled: "/api/ExcelImport/ImportTeacherDataFile?datatype=ed",
      videoFlaged: false,
      videoFlagDised: false,
      uploadUrleds: "/api/ExcelImport/ImportTeacherDataFile?datatype=eds",
      videoFlageds: false,
      videoFlagDiseds: false,
      uploadUrledsa: "/api/ExcelImport/ImportTeacherDataFile?datatype=edsa",
      videoFlagedsa: false,
      videoFlagDisedsa: false,
      totalTime: 10, //
      clock: null,
      canClick: true,
      currentOpe:"",
    };
  },
  methods: {
    beforeUploadVideo(file) {
      const isLt10M = file.size / 1024 / 1024 < 10;

      if (!isLt10M) {
        this.$message.error("上传文件大小不能超过10MB哦!");
        return false;
      }
    },
    uploadVideoProcessT(event, file, fileList) {
        this.currentOpe="t";
      this.videoFlagt = true;
      this.sendCode(8);
    },
    uploadVideoProcessS(event, file, fileList) {
        this.currentOpe="s";

      this.videoFlags = true;
      this.sendCode(5);
    },
    uploadVideoProcessES(event, file, fileList) {
        this.currentOpe="es";
      this.videoFlages = true;
      this.sendCode(30);
    },
    uploadVideoProcessBS(event, file, fileList) {
        this.currentOpe="bs";
      this.videoFlagbs = true;
      this.sendCode(30);
    },
    uploadVideoProcessED(event, file, fileList) {
        this.currentOpe="ed";
      this.videoFlaged = true;
      this.sendCode(25);
    },
    uploadVideoProcessEDS(event, file, fileList) {
        this.currentOpe="eds";
      this.videoFlageds = true;
      this.sendCode(45);
    },
    uploadVideoProcessEDSA(event, file, fileList) {
        this.currentOpe="edsa";
      this.videoFlageds = true;
      this.sendCode(5);
    },
    handleVideoSuccess(res, file) {
      //获取上传图片地址
      this.videoFlagt = false;
      this.videoFlags = false;
      this.videoFlages = false;
      this.videoFlagbs = false;
      this.videoFlaged = false;
      this.videoFlageds = false;
      this.videoFlagedsa = false;

      //当倒计时小于0时清除定时器
      window.clearInterval(this.clock); //关闭
      this.percentage = 0;
      this.canClick = true; //这里重新开启

      if (res.success) {
          switch (this.currentOpe) {
              case "t":
                  this.videoFlagDist=true;
                  break;
              case "s":
                  this.videoFlagDiss=true;
                  break;
              case "ed":
                  this.videoFlagDised=true;
                  break;
              case "es":
                  this.videoFlagDises=true;
                  break;
              case "bs":
                  this.videoFlagDisbs=true;
                  break;
              case "eds":
                  this.videoFlagDiseds=true;
                  break;
              case "edsa":
                  this.videoFlagDisedsa=true;
                  break;
          
              default:
                  break;
          }
        this.$message.success("处理成功！");
      } else {
        this.$message.error("文件上传失败，请重新上传！");
      }
    },
    sendCode(xd = 10) {
      if (!this.canClick) {
        return;
      } //节流
      this.canClick = false;
      this.clock = window.setInterval(() => {
        var pval = parseInt(this.percentage + 1 * (100 / xd));
        pval = pval > 98 ? 99 : pval;
        this.percentage = pval;
      }, 1000);
    }
  },
  mounted() {}
};
</script>
<style scoped>
.box-card {
  margin-top: 20px;
}
.note-alert {
  margin: 0 20px;
  border-bottom: 1px dashed #ccc;
  padding-bottom: 20px;
}
</style>
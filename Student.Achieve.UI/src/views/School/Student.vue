<template>
  <section>
    <!--工具条-->
    <el-col :span="24" class="toolbar" style="padding-bottom: 0px;">
      <el-form :inline="true" :model="filters" @submit.native.prevent>
        <el-form-item>
          <el-input v-model="filters.name" placeholder="输入查询内容"></el-input>
        </el-form-item>
        <el-form-item>
          <el-button type="primary" @click="getStudents">查询</el-button>
        </el-form-item>
        <el-form-item>
          <el-button type="primary" @click="addStudents">新增</el-button>
        </el-form-item>
      </el-form>
    </el-col>

    <!--列表-->
    <el-table
      :data="Students"
      highlight-current-row
      v-loading="listLoading"
      @selection-change="selsChange"
      style="width: 100%;"
    >
      <el-table-column type="selection" width="50"></el-table-column>
      <el-table-column type="index" width="80"></el-table-column>
      <el-table-column prop="grade.Name" label="年级" width sortable></el-table-column>
      <el-table-column prop="clazz.Name" label="班级" width sortable></el-table-column>
      <el-table-column prop="StudentNo" label="学号" width sortable></el-table-column>
      <el-table-column prop="Name" label="姓名" width sortable></el-table-column>
      <el-table-column prop="SubjectA" label="学科一" width sortable></el-table-column>
      <el-table-column prop="SubjectB" label="学科二" width sortable></el-table-column>
      <el-table-column prop="InSchoolSituation" label="在校情况" width sortable></el-table-column>
      <el-table-column prop="EnrollmentYear" label="入学年份" width sortable></el-table-column>
      <el-table-column prop="Gender" label="性别" width sortable></el-table-column>
      <el-table-column prop="IsIndicators" label="是否指标" width sortable></el-table-column>

      <el-table-column label="操作" width="150">
        <template scope="scope">
          <el-button size="small" @click="editStudents(scope.$index, scope.row)">编辑</el-button>
          <el-button type="danger" size="small" @click="deleteStudents(scope.$index, scope.row)">删除</el-button>
        </template>
      </el-table-column>
    </el-table>

    <!--工具条-->
    <el-col :span="24" class="toolbar">
      <el-button type="danger" @click="batchRemove" :disabled="this.sels.length===0">批量删除</el-button>
      <el-pagination
        layout="prev, pager, next"
        @current-change="nextPageStudents"
        :page-size="50"
        :total="total"
        style="float:right;"
      ></el-pagination>
    </el-col>

    <!--新增界面-->
    <el-dialog
      title="新增学生"
      :visible.sync="addFormVisible"
      v-model="addFormVisible"
      :close-on-click-modal="false"
    >
      <el-form :model="addForm" label-width="80px" :rules="addFormRules" ref="addForm">
        <el-form-item label="年级" prop="gradeid">
          <el-select @change="getClazzTreeFunc" v-model="addForm.gradeid" placeholder="请选择">
            <el-option
              v-for="item in GradeTree"
              :key="item.value"
              :label="item.label"
              :value="item.value"
            ></el-option>
          </el-select>
        </el-form-item>

        <el-form-item label="班级" prop="clazzid">
          <el-select v-model="addForm.clazzid" placeholder="请选择">
            <el-option
              v-for="item in ClazzTree"
              :key="item.value"
              :label="item.label"
              :value="item.value"
            ></el-option>
          </el-select>
        </el-form-item>

        <el-form-item label="学号" prop="StudentNo">
          <el-input v-model="addForm.StudentNo" auto-complete="off"></el-input>
        </el-form-item>
        <el-form-item label="姓名" prop="Name">
          <el-input v-model="addForm.Name" auto-complete="off"></el-input>
        </el-form-item>
        <el-form-item label="学科一" prop="SubjectA">
          <el-input v-model="addForm.SubjectA" auto-complete="off"></el-input>
        </el-form-item>
        <el-form-item label="学科二" prop="SubjectB">
          <el-input v-model="addForm.SubjectB" auto-complete="off"></el-input>
        </el-form-item>
        <el-form-item label="在校情况" prop="InSchoolSituation">
          <el-input v-model="addForm.InSchoolSituation" auto-complete="off"></el-input>
        </el-form-item>
        <el-form-item label="是否指标" prop="IsIndicators">
          <el-select v-model="addForm.IsIndicators" placeholder="请选择">
            <el-option  label="是" value="是"></el-option>
            <el-option   label="否" value="否"></el-option>
          </el-select>
        </el-form-item>
        <el-form-item label="入学年份" prop="EnrollmentYear">
          <el-input v-model="addForm.EnrollmentYear" auto-complete="off"></el-input>
        </el-form-item>
        <el-form-item label="性别" prop="Gender">
 <el-select v-model="addForm.Gender" placeholder="请选择">
            <el-option  label="男" value="男"></el-option>
            <el-option   label="女" value="女"></el-option>
          </el-select>
        </el-form-item>
      </el-form>
      <div slot="footer" class="dialog-footer">
        <el-button @click.native="addFormVisible = false">取消</el-button>
        <el-button type="primary" @click.native="addSubmit" :loading="addLoading">提交</el-button>
      </div>
    </el-dialog>
    <!--编辑界面-->
    <el-dialog
      title="编辑学生"
      :visible.sync="editFormVisible"
      v-model="editFormVisible"
      :close-on-click-modal="false"
    >
      <el-form :model="editForm" label-width="80px" :rules="editFormRules" ref="editForm">
        <el-form-item label="年级" prop="gradeid">
          <el-select @change="getClazzTreeFunc" v-model="editForm.gradeid" placeholder="请选择">
            <el-option
              v-for="item in GradeTree"
              :key="item.value"
              :label="item.label"
              :value="item.value"
            ></el-option>
          </el-select>
        </el-form-item>

        <el-form-item label="班级" prop="clazzid">
          <el-select v-model="editForm.clazzid" placeholder="请选择">
            <el-option
              v-for="item in ClazzTree"
              :key="item.value"
              :label="item.label"
              :value="item.value"
            ></el-option>
          </el-select>
        </el-form-item>

        <el-form-item label="学号" prop="StudentNo">
          <el-input v-model="editForm.StudentNo" auto-complete="off"></el-input>
        </el-form-item>
        <el-form-item label="姓名" prop="Name">
          <el-input v-model="editForm.Name" auto-complete="off"></el-input>
        </el-form-item>
        <el-form-item label="学科一" prop="SubjectA">
          <el-input v-model="editForm.SubjectA" auto-complete="off"></el-input>
        </el-form-item>
        <el-form-item label="学科二" prop="SubjectB">
          <el-input v-model="editForm.SubjectB" auto-complete="off"></el-input>
        </el-form-item>
        <el-form-item label="在校情况" prop="InSchoolSituation">
          <el-input v-model="editForm.InSchoolSituation" auto-complete="off"></el-input>
        </el-form-item>
        <el-form-item label="是否指标" prop="IsIndicators">
          <el-select v-model="editForm.IsIndicators" placeholder="请选择">
            <el-option  label="是" value="是"></el-option>
            <el-option   label="否" value="否"></el-option>
          </el-select>
        </el-form-item>
        <el-form-item label="入学年份" prop="EnrollmentYear">
          <el-input v-model="editForm.EnrollmentYear" auto-complete="off"></el-input>
        </el-form-item>
        <el-form-item label="性别" prop="Gender">
          <el-select v-model="editForm.Gender" placeholder="请选择">
            <el-option  label="男" value="男"></el-option>
            <el-option   label="女" value="女"></el-option>
          </el-select>
        </el-form-item>
      </el-form>
      <div slot="footer" class="dialog-footer">
        <el-button @click.native="editFormVisible = false">取消</el-button>
        <el-button type="primary" @click.native="editSubmit" :loading="editLoading">提交</el-button>
      </div>
    </el-dialog>
  </section>
</template>

<script>
import util from "../../../util/date";
import {
  addStudents,
  getStudentsListPage,
  removeStudents,
  editStudents,
  getGradeTree,
  getClazzTree
} from "../../api/api";

export default {
  data() {
    return {
      filters: {
        name: ""
      },
      Students: [],
      GradeTree: [],
      ClazzTree: [],
      total: 0,
      page: 1,
      listLoading: false,
      sels: [], //列表选中列
      editFormVisible: false, //编辑界面是否显示
      editLoading: false,
      editFormRules: {
        StudentNo: [{ required: true, message: "请输入学号", trigger: "blur" }],
        Name: [{ required: true, message: "请输入姓名", trigger: "blur" }],
        SubjectA: [
          { required: true, message: "请输入学科一", trigger: "blur" }
        ],
        SubjectB: [
          { required: true, message: "请输入学科二", trigger: "blur" }
        ],
        InSchoolSituation: [
          { required: true, message: "请输入在校情况", trigger: "blur" }
        ],
        Gender: [{ required: true, message: "请输入性别", trigger: "blur" }],
        EnrollmentYear: [
          { required: true, message: "请输入入学年份", trigger: "blur" }
        ]
      },
      //编辑界面数据
      editForm: {
        Id: 0,
        StudentNo: "",
        Name: "",
        SubjectA: "",
        SubjectB: "",
        IsIndicators: "",
        Gender: "",
        InSchoolSituation: "",
        GradeId: 0,
        ClazzId: 0,
        EnrollmentYear:""
      },
      addFormVisible: false, //编辑界面是否显示
      addLoading: false,
      addFormRules: {
        StudentNo: [{ required: true, message: "请输入学号", trigger: "blur" }],
        Name: [{ required: true, message: "请输入姓名", trigger: "blur" }],
        SubjectA: [
          { required: true, message: "请输入学科一", trigger: "blur" }
        ],
        SubjectB: [
          { required: true, message: "请输入学科二", trigger: "blur" }
        ],
        InSchoolSituation: [
          { required: true, message: "请输入在校情况", trigger: "blur" }
        ],
        Gender: [{ required: true, message: "请输入性别", trigger: "blur" }],
        EnrollmentYear: [
          { required: true, message: "请输入入学年份", trigger: "blur" }
        ]
      },
      //编辑界面数据
      addForm: {
        Id: 0,
        StudentNo: "",
        Name: "",
        SubjectA: "",
        SubjectB: "",
        IsIndicators: "",
        Gender: "",
        InSchoolSituation: "",
        GradeId: 0,
        ClazzId: 0,
        EnrollmentYear:""
      }
    };
  },
  methods: {
    //性别显示转换
    formatSex: function(row, column) {
      return row.sex == 1 ? "男" : row.sex == 0 ? "女" : "未知";
    },
    formatBirth: function(row, column) {
      return !row.birth || row.birth == ""
        ? ""
        : util.formatDate.format(new Date(row.birth), "yyyy-MM-dd");
    },
    nextPageStudents(val) {
      this.page = val;
      this.getStudents();
    },
    //获取用户列表
    getStudents() {
      let para = {
        page: this.page,
        key: this.filters.name
      };
      this.listLoading = true;

      getStudentsListPage(para).then(res => {
        this.total = res.data.response.dataCount;
        this.Students = res.data.response.data;
        this.listLoading = false;
      });
    },
    //删除
    deleteStudents: function(index, row) {
      this.$confirm("确认删除该记录吗?", "提示", {
        type: "warning"
      })
        .then(() => {
          this.listLoading = true;
          //NProgress.start();
          let para = { id: row.Id };
          removeStudents(para).then(res => {
            if (util.isEmt.format(res)) {
              this.listLoading = false;
              return;
            }
            this.listLoading = false;
            //NProgress.done();
            if (res.data.success) {
              this.$message({
                message: "删除成功",
                type: "success"
              });
            } else {
              this.$message({
                message: res.data.msg,
                type: "error"
              });
            }

            this.getStudents();
          });
        })
        .catch(() => {});
    },
    //显示编辑界面
    editStudents: function(index, row) {
      this.editFormVisible = true;
      this.editForm = Object.assign({}, row);

      this.ClazzTree = [];
      let para = { gid: row.gradeid };
      getClazzTree(para).then(res => {
        this.ClazzTree = res.data.response;
      });
    },
    //编辑
    editSubmit: function() {
      this.$refs.editForm.validate(valid => {
        if (valid) {
          this.$confirm("确认提交吗？", "提示", {}).then(() => {
            this.editLoading = true;
            //NProgress.start();
            let para = Object.assign({}, this.editForm);

            editStudents(para).then(res => {
              if (util.isEmt.format(res)) {
                this.editLoading = false;
                return;
              }
              if (res.data.success) {
                this.editLoading = false;
                //NProgress.done();
                this.$message({
                  message: res.data.msg,
                  type: "success"
                });
                this.$refs["editForm"].resetFields();
                this.editFormVisible = false;
                this.getStudents();
              } else {
                this.$message({
                  message: res.data.msg,
                  type: "error"
                });
              }
            });
          });
        }
      });
    },
    //显示新增界面
    addStudents() {
      this.addFormVisible = true;
      this.addForm = {};
      this.ClazzTree = [];
    },
    getClazzTreeFunc() {
      var gid = this.editForm.gradeid>0?this.editForm.gradeid:this.addForm.gradeid;

      let para = { gid: gid };
      getClazzTree(para).then(res => {
        this.ClazzTree = res.data.response;
      });
    },
    //新增
    addSubmit: function() {
      this.$refs.addForm.validate(valid => {
        if (valid) {
          this.$confirm("确认提交吗？", "提示", {}).then(() => {
            this.addLoading = true;
            let para = Object.assign({}, this.addForm);
            addStudents(para).then(res => {
              if (util.isEmt.format(res)) {
                this.addLoading = false;
                return;
              }

              if (res.data.success) {
                this.addLoading = false;
                //NProgress.done();
                this.$message({
                  message: res.data.msg,
                  type: "success"
                });
                this.$refs["addForm"].resetFields();
                this.addFormVisible = false;
                this.getStudents();
              } else {
                this.$message({
                  message: res.data.msg,
                  type: "error"
                });
              }
            });
          });
        }
      });
    },
    selsChange: function(sels) {
      this.sels = sels;
    },
    //批量删除
    batchRemove: function() {
      // return;

      var ids = this.sels.map(item => item.Id).toString();
      this.$confirm("确认删除选中记录吗？", "提示", {
        type: "warning"
      })
        .then(() => {
          this.listLoading = true;
          //NProgress.start();
          let para = { ids: ids };

          batchRemoveStudents(para).then(res => {
            this.listLoading = false;
            //NProgress.done();
            this.$message({
              message: "该功能未开放",
              type: "warning"
            });
            console.log(res);
          });
        })
        .catch(() => {});
    }
  },
  mounted() {
    this.getStudents();

    let para = {};
    getGradeTree(para).then(res => {
      this.GradeTree = res.data.response;
    });
  }
};
</script>

<style scoped>
</style>

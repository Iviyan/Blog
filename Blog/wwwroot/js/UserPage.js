const commentsPerReuest = 20;
const postsPerReuest = 20;

async function get(url, params = {}) {
	var query = Object.keys(params)
		.filter(o => params[o] != null)
		.map(k => encodeURIComponent(k) + '=' + encodeURIComponent(params[k]))
		.join('&');

	return await fetch(url + '?' + query);
}

async function getj(url, params = {}) {
	return await (await get(url, params)).json();
}

async function post(url, params = {}) {
	return await fetch(url,
		{
			method: 'POST',
			body: JSON.stringify(params),
			headers: {
				'Content-Type': 'application/json'
			}
		});
}

async function postj(url, params = {}) {
	return await (await post(url, params)).json();
}

async function fdelete(url) {
	return await fetch(url, { method: 'DELETE' });
}

async function put(url, params = {}) {
	return await fetch(url,
		{
			method: 'PUT',
			body: JSON.stringify(params),
			headers: {
				'Content-Type': 'application/json'
			}
		});
}

const app = Vue.createApp({
	data() {
		return {
			posts: [],
			postCount: null,
			lastPostId: null,
			allPostsLoaded: false,
			postsLoading: true,
			postEditor: ClassicEditor,
			postEditorData: '',
			postEditorConfig: {
				//plugins: [ SimpleUploadAdapter ],
				placeholder: "Текст...",
				language: 'ru',
				simpleUpload: {
					uploadUrl: '/images/upload',
					withCredentials: true
					//headers: {
					//	'X-CSRF-TOKEN': 'CSRF-Token',
					//	Authorization: 'Bearer <JSON Web Token>'
					//}
				}
			},
			isSendPostButtonEnabled: true
		}
	},
	methods: {
		async loadPosts() {
			let data = await getj('/api/posts', {
				userId: this.userId,
				count: postsPerReuest,
				lastId: this.lastPostId
			});
			let items = data.items.map(p => {
				let user = data.users.find(u => u.id == p.user_id);
				return {
					id: p.id,
					author: user,
					date: p.creation_date,
					text: p.body,
					initialCommentCount: p.comment_count,
					comments: []
				};
			});

			this.postCount = data.count;

			if (data.items.length > 0) {
				this.posts.push(...items);
				this.lastPostId = data.items.slice(-1)[0].id;
				if (this.postCount == this.posts.length || data.items.count < postsPerReuest)
					this.allPostsLoaded = true;
			}
			else
				this.allPostsLoaded = true;

			this.postsLoading = false;
		},
		async sendPost() {
			if (this.postEditorData.match(/^\s*$/)) return;
			this.isSendPostButtonEnabled = false;

			let responce = await post('/api/posts', {
				text: this.postEditorData
			});

			if (responce.status != 200) {
				this.isSendPostButtonEnabled = true;
				return; // todo
			};
			let data = await responce.json();

			this.posts.unshift({
				id: data.id,
				author: this.authUser,
				date: data.creation_date,
				text: data.body
			});

			console.log(this.posts);
			this.postEditorData = "";
			this.isSendPostButtonEnabled = true;
		},
		async deletePost(id) {
			let responce = await fdelete('/api/posts/' + id);

			if (responce.status != 200) {
				return; // todo
			};

			let postIndex = this.posts.findIndex(p => p.id == id);
			this.posts.splice(postIndex, 1);
		},
		async updatePost(data) {
			let responce = await put('/api/posts/' + data.id, {
				text: data.text
			});

			if (responce.status != 200) {
				return; // todo
			};

			let post = this.posts.find(p => p.id == data.id);
			post.text = data.text;
		}
	},
	created() {
		this.loadPosts();
	}
})
	.use(CKEditor);

app.directive('autosize', {
	mounted(el) {
		el.setAttribute("style", "height:" + (el.scrollHeight) + "px;overflow-y:hidden;");
		function OnInput() {
			this.style.height = "auto";
			this.style.height = (this.scrollHeight) + "px";
		}
		el.addEventListener("input", OnInput, false);
	}
})

app.directive('focus', {
	mounted(el) {
		el.focus()
	}
})

app.component('post', {
	props: {
		id: { type: Number, required: true },
		author: { type: Object, required: true },
		date: { type: String, required: true },
		text: { type: String, required: true },
		initialCommentCount: { type: Number, default: 0 },
		comments: { type: Array, default: () => [] }
	},
	emits: {
		delete: (id) => typeof (id) == 'number',
		edit: (id, text) => typeof (id) == 'number',
	},
	data() {
		return {
			commentCount: this.initialCommentCount,
			lastCommentId: null,
			allCommentsLoaded: false,
			commentsVisible: true,
			commentsLoading: false,
			editMode: false,
			postEditor: ClassicEditor,
			postEditorData: this.text,
			postEditorConfig: {
				placeholder: "Текст...",
				language: 'ru'
			},
			commentText: "",
			isSendCommentButtonEnabled: true
		}
	},
	computed: {
		authorLink() { return `/u/${this.author.login ?? this.author.id}`; }
	},
	methods: {
		async loadComments() {
			console.log(this.lastCommentId);
			let data = await getj('/api/comments', {
				postId: this.id,
				count: commentsPerReuest,
				lastId: this.lastCommentId
			});
			let items = data.items.map(p => {
				let user = data.users.find(u => u.id == p.user_id);
				return {
					id: p.id,
					author: user,
					date: p.creation_date,
					text: p.body
				};
			});

			this.commentCount = data.count;

			if (data.items.length > 0) {
				this.comments.push(...items);
				this.lastCommentId = data.items.slice(-1)[0].id;
				if (this.commentCount == this.comments.length || data.items.count < commentsPerReuest)
					this.allCommentsLoaded = true;
			}
			else
				this.allCommentsLoaded = true;

			this.commentsLoading = false;

			console.log(this);
		},
		async sendComment() {
			if (this.commentText.match(/^\s*$/)) return;
			this.isSendCommentButtonEnabled = false;

			let responce = await post('/api/comments', {
				post_id: this.id,
				text: this.commentText
			});

			if (responce.status != 200) {
				this.isSendCommentButtonEnabled = true;
				return; // todo
			};
			let data = await responce.json();

			this.comments.push({
				id: data.id,
				author: this.authUser,
				date: data.creation_date,
				text: data.body
			});

			this.commentCount++;

			this.commentText = "";
			this.isSendCommentButtonEnabled = true;
		},
		async updateComment(data) {
			let responce = await put('/api/comments/' + data.id, {
				text: data.text
			});

			if (responce.status != 200) {
				return; // todo
			};

			let comment = this.comments.find(p => p.id == data.id);
			comment.text = data.text;
		},
		async deleteComment(id) {
			let responce = await fdelete('/api/comments/' + id);

			if (responce.status != 200) {
				return; // todo
			};

			let commentIndex = this.comments.findIndex(c => c.id == id);
			this.comments.splice(commentIndex, 1);

			this.commentCount--;
		}
	},
	created() {
		if (this.commentCount != 0) {
			this.commentsLoading = true;
			this.loadComments().then(() => {
				if (this.commentCount == this.comments.length)
					this.allCommentsLoaded = true;
			});
		} else
			this.allCommentsLoaded = true;

	},
	mounted() {
		//let el = this.$refs.commentTextInput;
		//setTextareaAutosize(el);
	},
	template:
		`<div class="post col border">
	<div class="align-content-around p-3 pb-2">
		<div class="post-topbar d-flex clearfix">
			<img :src="author.avatar" alt="">
			<div class="post-author flex-fill">
				<a :href="authorLink" v-html="author.full_name"></a>
				<span>{{date}}</span>
			</div>
			<div v-if="author.id === authUser?.id" class="post-options dropdown">
				<svg data-bs-toggle="dropdown" class="post-menu-icon action" xmlns="http://www.w3.org/2000/svg" width="16" height="16" fill="currentColor" viewBox="0 0 16 16">
					<path d="M9.5 13a1.5 1.5 0 1 1-3 0 1.5 1.5 0 0 1 3 0zm0-5a1.5 1.5 0 1 1-3 0 1.5 1.5 0 0 1 3 0zm0-5a1.5 1.5 0 1 1-3 0 1.5 1.5 0 0 1 3 0z" />
				</svg>
				<ul class="dropdown-menu">
					<li><button @@click="editMode = true" class="dropdown-item">Редактировать</button></li>
					<li><button @@click="$emit('delete', id)" class="dropdown-item">Удалить</button></li>
				</ul>
			</div>
		</div>
		<div v-if="!editMode" class="post-text mt-2" v-html="text"></div>
		<div v-else class="my-2">
			<ckeditor :editor="postEditor"
					  v-model="postEditorData"
					  :config="postEditorConfig"></ckeditor>
			<div class="d-flex justify-content-end mt-1">
				<button @@click="editMode = false" class="btn btn-sm btn-outline-danger">Отмена</button>
				<button @@click="$emit('edit', { id: id, text: postEditorData }); editMode = false" class="btn ms-1 btn-sm btn-outline-success">Изменить</button>
			</div>
		</div>
		<div class="post-feedback pt-2 border-top">
			<button @@click="commentsVisible = !commentsVisible" class="comments-button text-secondary">Комментарии  {{commentCount}}</button>
		</div>
	</div>
	<div v-if="comments.length > 0 || isAuth" v-show="commentsVisible" class="post-comments py-2 px-3 border-top px-0">
		<div v-if="comments.length > 0" class="row row-cols-1 gy-3 gx-0">
			<comment v-for="comment in comments" v-bind="comment" :key="comment.id" @@delete="deleteComment" @@edit="updateComment"></comment>
		</div>
		<div class="col text-center">
			<button v-show="!allCommentsLoaded && !commentsLoading" @@click="loadComments" class="btn">Загрузить ещё</button>
			<div v-show="commentsLoading" class="spinner-border" role="status">
				<span class="visually-hidden">Загрузка...</span>
			</div>
		</div>
		<div v-if="isAuth" class="write-comment d-flex mt-2">
			<img class="rounded-circle " :src="authUser.avatar" width="32" height="32" />
			<textarea
				ref="commentTextInput"
				v-autosize
				v-model="commentText"
				placeholder="Текст..."
				class="form-control flex-fill mx-2"
				rows="1"
				/>

			<button @@click="sendComment" :disabled="!isSendCommentButtonEnabled" class="send-comment-button">
				<svg xmlns="http://www.w3.org/2000/svg" width="22" height="22" fill="currentColor" class="action" viewBox="0 0 16 16">
					<path d="M15.854.146a.5.5 0 0 1 .11.54l-5.819 14.547a.75.75 0 0 1-1.329.124l-3.178-4.995L.643 7.184a.75.75 0 0 1 .124-1.33L15.314.037a.5.5 0 0 1 .54.11ZM6.636 10.07l2.761 4.338L14.13 2.576 6.636 10.07Zm6.787-8.201L1.591 6.602l4.339 2.76 7.494-7.493Z"></path>
				</svg>
			</button>
		</div>
	</div>
</div>`
});

app.component('comment', {
	// camelCase в JavaScript
	props: {
		id: { type: Number, required: true },
		author: { type: Object, required: true },
		date: { type: String, required: true },
		text: { type: String, required: true }
	},
	emits: {
		delete: (id) => typeof (id) == 'number'
	},
	data() {
		return {
			editMode: false,
			editData: this.text
		}
	},
	computed: {
		authorLink() { return `/u/${this.author.login ?? this.author.id}`; }
	},
	methods: {
		async editSubmit() {
			await this.$emit('edit', { id: this.id, text: this.editData });
			this.editMode = false
		}
	},
	template:
		`<div class="col comment d-flex flex-row">
	<img class="rounded-circle me-2" :src="author.avatar" width="32" height="32" />
	<div class="d-flex flex-column flex-fill">
		<div class="d-flex flex-row flex-fill">
			<div class="comment-author flex-fill me-2">
				<a :href="authorLink" v-html="author.full_name"></a>
				<span class="ms-2">{{date}}</span>
			</div>
			<div v-if="author.id === authUser?.id" class="row gx-1 gy-0">
				<div class="col">
					<svg @@click="editMode = !editMode" xmlns="http://www.w3.org/2000/svg" width="16" height="16" fill="currentColor" class="action" viewBox="0 0 16 16">
						<path d="M12.146.146a.5.5 0 0 1 .708 0l3 3a.5.5 0 0 1 0 .708l-10 10a.5.5 0 0 1-.168.11l-5 2a.5.5 0 0 1-.65-.65l2-5a.5.5 0 0 1 .11-.168l10-10zM11.207 2.5 13.5 4.793 14.793 3.5 12.5 1.207 11.207 2.5zm1.586 3L10.5 3.207 4 9.707V10h.5a.5.5 0 0 1 .5.5v.5h.5a.5.5 0 0 1 .5.5v.5h.293l6.5-6.5zm-9.761 5.175-.106.106-1.528 3.821 3.821-1.528.106-.106A.5.5 0 0 1 5 12.5V12h-.5a.5.5 0 0 1-.5-.5V11h-.5a.5.5 0 0 1-.468-.325z" />
					</svg>
				</div>
				<div class="col">
					<svg @@click="$emit('delete', id)" xmlns="http://www.w3.org/2000/svg" width="16" height="16" fill="currentColor" class="action" viewBox="0 0 16 16">
						<path fill-rule="evenodd" d="M13.854 2.146a.5.5 0 0 1 0 .708l-11 11a.5.5 0 0 1-.708-.708l11-11a.5.5 0 0 1 .708 0Z"/>
						<path fill-rule="evenodd" d="M2.146 2.146a.5.5 0 0 0 0 .708l11 11a.5.5 0 0 0 .708-.708l-11-11a.5.5 0 0 0-.708 0Z"/>
					</svg>
				</div>
			</div>
		</div>
		<div v-if="!editMode" class="comment-text">{{text}}</div>
		<div v-else class="my-1 comment-edit">
			<textarea v-autosize
					  v-focus
					  rows="1"
					  v-model="editData"
					  class="form-control flex-fill mx-2"></textarea>
			<div class="d-flex justify-content-end mt-1">
				<button @@click="editMode = false" class="btn btn-sm btn-outline-danger">Отмена</button>
				<button @@click="editSubmit" class="btn ms-1 btn-sm btn-outline-success">Изменить</button>
			</div>
		</div>
	</div>
</div>`
});

(async () => {
	let authUserResponce = await get('/api/mybaseinfo');

	if (authUserResponce.status == 200) {
		let authUser = await authUserResponce.json();
		app.config.globalProperties.authUser = authUser;
	} else
		app.config.globalProperties.authUser = null;
	app.config.globalProperties.isAuth = authUserResponce.status == 200;

	app.config.globalProperties.userId = __userId;

	const vm = app.mount('#container');
})();
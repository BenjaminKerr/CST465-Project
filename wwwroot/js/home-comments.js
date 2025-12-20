(function () {
  async function fetchComments(visualizationId) {
    const res = await fetch(`/api/comments?visualizationId=${visualizationId}`);
    if (!res.ok) return [];
    return await res.json();
  }

  function renderComments(container, comments) {
    container.innerHTML = '';
    if (!comments.length) {
      container.innerHTML = '<p class="text-muted">No comments yet — be the first to comment.</p>';
      return;
    }
    const list = document.createElement('div');
    list.className = 'comment-list';
    comments.forEach(c => {
      const el = document.createElement('div');
      el.className = 'comment-item card mb-2';

      const author = c.authorEmail || c.AuthorEmail || 'Anonymous';
      const date = new Date(c.createdAt || c.CreatedAt).toLocaleString();
      const content = c.content || c.Content || '';

      el.innerHTML = `
        <div class="card-body">
          <div class="d-flex justify-content-between align-items-start mb-2">
            <strong>${escapeHtml(author)}</strong>
            <small class="text-muted">${date}</small>
          </div>
          <div>${escapeHtml(content)}</div>
        </div>`;
      list.appendChild(el);
    });
    container.appendChild(list);
  }

  function escapeHtml(s) {
    if (!s) return '';
    return s.replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;').replace(/"/g, '&quot;');
  }

  async function postComment(dto) {
    const token = document.querySelector('meta[name="x-xsrf-token"]')?.getAttribute('content');
    const headers = { 'Content-Type': 'application/json' };
    if (token) headers['RequestVerificationToken'] = token;

    const res = await fetch('/api/comments', {
      method: 'POST',
      headers,
      body: JSON.stringify(dto)
    });
    return res;
  }

  document.addEventListener('DOMContentLoaded', function () {
    const viz = document.querySelector('.visualization-placeholder');
    if (!viz) return;

    let commentsContainer = document.getElementById('commentsContainer');
    if (!commentsContainer) {
      commentsContainer = document.createElement('div');
      commentsContainer.id = 'commentsContainer';
      commentsContainer.className = 'comments-section';
      viz.appendChild(commentsContainer);
    }

    let form = document.querySelector('.comment-form');
    // if (!form) {
    //   form = document.createElement('form');
    //   form.className = 'comment-form mt-3';
    //   form.innerHTML = `
    //     <h4>Comments</h4>
    //     <div class="mb-2">
    //       <label class="form-label">Posting as:</label>
    //       <input type="text" class="form-control bg-light" value="Logged-in User" readonly />
    //     </div>
    //     <div class="mb-2">
    //       <label for="commentContent" class="form-label">Comment</label>
    //       <textarea id="commentContent" name="Content" class="form-control" required maxlength="1000" rows="3"></textarea>
    //     </div>
    //     <div>
    //       <button class="btn btn-primary" type="submit">Post Comment</button>
    //     </div>
    //   `;
    //   viz.appendChild(form);
    // }

    const dbSizeInput = document.getElementById('dbSize');
    const getVisualizationId = () => parseInt(dbSizeInput?.value || '8');

    async function load() {
      const vid = getVisualizationId();
      const comments = await fetchComments(vid);
      renderComments(commentsContainer, comments);
      const hid = form.querySelector('[name="VisualizationId"]');
      if (hid) hid.value = vid;
    }

    form.addEventListener('submit', async function (e) {
      e.preventDefault();

      const contentEl = form.querySelector('[name="Content"]') || form.querySelector('#commentContent');
      const content = (contentEl && contentEl.value || '').trim();

      if (!content) return;

      const dto = {
        visualizationId: getVisualizationId(),
        content: content
      };

      const res = await postComment(dto);
      if (res.ok) {
        if (contentEl) contentEl.value = '';
        await load();
      } else {
        alert('Failed to post comment.');
      }
    });

    dbSizeInput?.addEventListener('change', load);
    load();
  });
})();